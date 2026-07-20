## 图书馆管理系统

### 0.准备阶段
- 技术栈vue3(js) + C# + Oracle 19c
- vue3自学网站 https://cn.vuejs.org/guide/quick-start
- C#自学网站 https://learn.microsoft.com/zh-cn/dotnet/csharp/


### 1.运行方法
#### 1.环境配置

- 需要Node.js 22.14.0 

    下载地址 https://nodejs.org/zh-cn

    检验，输出版本则正确
    ```
    node -v
    npm -v
    ```

 - .net8.0
    下载并运行exe
    https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.412/dotnet-sdk-8.0.412-win-x64.exe

 - 下载redis,/backend运行
   ```
   dotnet add package StackExchange.Redis
   ```

 - /frontend运行
   ```
   npm install
   npm install axios
   npm install swiper@^10
   npm install -D tailwindcss postcss autoprefixer
   npx tailwindcss init -p
   npm install jwt-decode
   ```

#### 2.开发环境启动方式 前端后端都运行在本地+数据库运行在远程服务器（大家在window上这样启动）

- 前端
```
cd frontend
npm run dev    # 自动读取 .env.development
```

- 后端
```
# Windows PowerShell
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

- 数据库代码运行方式

方案一 (图形化，推荐)vscode在项目中写完sql语句，拖拽到sql develop运行，sql develop上执行的代码就是服务器执行的代码

方案二 (命令行)vscode远程连接服务器控制台运行sql代码

#### 3.部署后服务器上启动方式
- 前端
```
cd frontend
npm run build     # 读取 .env.production
```

- 后端
```
export ASPNETCORE_ENVIRONMENT=Production
dotnet backend.dll
```

## 答辩要点：业务逻辑
- 不能只有表增删改
- 触发器 表之间联动