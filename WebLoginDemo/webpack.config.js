const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CssMinimizerPlugin = require('css-minimizer-webpack-plugin');
const TerserPlugin = require('terser-webpack-plugin');

module.exports = {
    mode: 'development',//����ģʽ
    //mode: 'production',//�Ż��������͹������ܵ�ģʽ
    entry: {
        'site': './Script/init.ts'
    },
    output: {
        path: path.resolve(__dirname, 'wwwroot/dist'), // ���·��
        filename: 'js/[name].js', // ����ļ���
        clean: true, // ���Ŀ��Ŀ¼
    },
    resolve: {
        //�������Ϳ�����Ϊģ�鱻����
        extensions: [".ts", ".tsx", ".js"],
        alias: {
            "@": path.resolve(__dirname, "."),
        },
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/, // ƥ�� TypeScript �ļ�
                use: "ts-loader", // ʹ�� ts-loader ���� TypeScript �ļ�
                exclude: /node_modules/ // �ų� node_modules �ļ���
            },
            {
                test: /\.(css|scss)$/,
                use: [
                    {
                        loader: MiniCssExtractPlugin.loader,
                        options: {
                            esModule: true,
                        },
                    },
                    "css-loader",//����css�ļ�
                    'sass-loader',//����SCSS�ļ�
                ],
            }
        ],
    },
    plugins: [
        new MiniCssExtractPlugin(
            {
                filename: 'css/[name].css',
            }
        ),
    ],
    optimization: {
        minimize: true, // ��������ѹ��
        minimizer: [
            `...`,
            new CssMinimizerPlugin({
                minimizerOptions: {
                    preset: [
                        'default',
                        {
                            discardComments: { removeAll: true },
                        },
                    ],
                },
            }), // ѹ��css
            new TerserPlugin({
                terserOptions: {
                    format: {
                        comments: false,      // ѹ��ʱɾ������ע��
                    },
                },
                extractComments: false,   // ����ȡ *.LICENSE.txt
            }), // ѹ��js
        ],
    },
};