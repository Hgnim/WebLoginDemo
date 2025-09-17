using WebLoginDemo.Models;
using WebLoginDemo.Models.database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using static WebLoginDemo.Const.FilePath;
using static WebLoginDemo.DataCore;

namespace WebLoginDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
			Console.WriteLine(
@$"���������"
				);
			try {
				static void saveData() => ConfigModel.SaveData(configFile, config);

				if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

				if (!File.Exists(configFile))
					saveData();
				else
					config=ConfigModel.ReadData(configFile);

				if (config.UpdateConfig == true) {
					config.UpdateConfig = false;
					saveData();
					Console.WriteLine("�����ļ��Ѹ��£����˳������");
					return;
				}
			} catch { Console.WriteLine("���������ļ�ʱ���ִ���!"); return; }


			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddControllersWithViews();

			builder.WebHost.UseUrls(config.Website.Url.Get());
			builder.Services.AddHttpContextAccessor();
			builder.Services.AddSession();

			builder.Services.AddDbContext<ServerDbContext>(opt =>
				opt.UseSqlite("Data Source="+config.Website.Database.ConnectionString));

			#region Identity

			builder.Services.AddIdentity<IdentityUserModel, IdentityRole>()//��ʹ��AddDefaultIdentity��ʹ�ñ���ҳ�����߼�
				.AddEntityFrameworkStores<ServerDbContext>();
			    //.AddDefaultTokenProviders();//����Ҫ��Ӷ���������֤�ṩ�ĳ���

			builder.Services.Configure<IdentityOptions>(options => {
				// Password settings.
				options.Password.RequireDigit = false;//��Ҫ������
				options.Password.RequireLowercase = false;//��Ҫ��Сд��ĸ
				options.Password.RequireNonAlphanumeric = false;//��Ҫ�������ַ�
				options.Password.RequireUppercase = false;//��Ҫ���д��ĸ
				options.Password.RequiredLength = 6;//��С����Ϊ6
				options.Password.RequiredUniqueChars = 1;//Ҫ������1����ͬ�ַ�

				// Lockout settings.
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);//�˻�����ʱ��Ϊ5����
				options.Lockout.MaxFailedAccessAttempts = 5;//5�δ���������˻�
				options.Lockout.AllowedForNewUsers = true;//���û������˻�����

				// User settings.
				options.User.AllowedUserNameCharacters =
				"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
				options.User.RequireUniqueEmail = false;//���������Ψһ��

				// SignIn settings.
				options.SignIn.RequireConfirmedEmail = false;//��Ҫ������ȷ��
				options.SignIn.RequireConfirmedAccount = false;//��Ҫ���˻�ȷ��
			});


			builder.Services.ConfigureApplicationCookie(options => {
				// Cookie settings
				options.Cookie.HttpOnly = true;//ֻ��ͨ��HTTP����
				options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

				options.LoginPath = "/Account/Login";//��¼ҳ��
				options.AccessDeniedPath = "/Account/AccessDenied";
				options.SlidingExpiration = true;//����������
			});

			//����Identity��ҳ�棬���Razor Pages֧��
			builder.Services.AddRazorPages();
			#endregion

			var app = builder.Build();

			//����Identity��ҳ�棬����Razor Pages·��
			app.MapRazorPages();

			using (var scope = app.Services.CreateScope()) {
				//������������Զ��������ݿ��ļ�
				scope.ServiceProvider.GetRequiredService<ServerDbContext>().Database.EnsureCreated();

				scope.ServiceProvider.GetRequiredService<ServerDbContext>().Database.Migrate(); // �Զ�Ӧ��Ǩ��


				{
					//��鲢������ɫ
					RoleManager<IdentityRole> roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
					string[] roles = ["Owner", /*"Administrator", "Manager",*/ "User" ];
					foreach (var r in roles) {
						if (!await roleMgr.RoleExistsAsync(r))
							await roleMgr.CreateAsync(new IdentityRole(r));
					}
				}
				{
					//����Ĭ�Ϲ���Ա�û�
					string ownerUserName = "admin", ownerPassword = "Admin@0000",ownerRole = "Owner";
					UserManager<IdentityUserModel> userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUserModel>>();
					RoleManager<IdentityRole> roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

					//ȷ������Ա�û�����
					IdentityUserModel? admin = await userMgr.FindByNameAsync(ownerUserName);
					if (admin == null) {
						admin = new IdentityUserModel {
							UserName = ownerUserName,
							//EmailΪ�գ���ʹ������
						};

						IdentityResult createResult = await userMgr.CreateAsync(admin, ownerPassword);
						if (createResult.Succeeded)
							Console.WriteLine($"�Ѵ���Ĭ�Ϲ���Ա�û����û�����{ownerUserName}�����룺{ownerPassword}");
						else
							throw new Exception("��������Աʧ�ܣ�" + string.Join(", ", createResult.Errors));
					}

					//Ϊ����Ա�˻����ý�ɫ
					if (!await userMgr.IsInRoleAsync(admin, ownerRole)) {
						IdentityResult addRoleResult = await userMgr.AddToRoleAsync(admin, ownerRole);
						if (!addRoleResult.Succeeded)
							throw new Exception("������Ա�ӽ�ɫʧ�ܣ�" + string.Join(", ", addRoleResult.Errors));
					}
				}
			}

			app.UsePathBase(config.Website.Url.UrlRoot);
			app.UseSession();

			/*// Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }*/
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=StartPage}/{action=Index}"); // /{id?}");
			app.Run();
		}
	}
}
