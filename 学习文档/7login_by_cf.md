
前端新增模块
```
npm install jwt-decode
```
目的是前端能够解码jwt令牌判断其是否过期，以此决定是否需要重新登陆


已本地创建  提交 e5258f01 。 

> 本次提交完成了Reader的登录与注册和个人主页的展示与信息修改


**本次提交有以下需要注意：**

1. 必须安装以下以来才能运行
后端
```
dotnet add package BCrypt.Net-Next
```

```
dotnet add package StackExchange.Redis
```

```
dotnet add package System.IdentityModel.Tokens.Jwt

```

前端：

```
npm install jwt-decode
```

2. 本次提交在后端添加了JwtAuthenticationMiddleware和ExceptionMiddleware

- JwtAuthenticationMiddleware会拦截那些需要登陆后才能访问的接口，因此如果你们写的接口**不需要登陆就能访问**，请将其路径写在代码，具体如下：

**文件位置：backend/Common/Middleware/JwtAuthenticationMiddleware**
```c#
        private static readonly string[] _excludedPaths = new[]
        {
        "/api/login",
        "/api/register",          // 登录和注册接口
        "/api/docs",              //swagger UI 的根路径
        "/api/docs/index.html",   //swagger UI 的入口文件
        "/favicon.ico",           // 网站图标
    };
```
> 写在上面表示**以上路由及其子路径**不需要登录，如果只想放行特定路由而不放行它的子路径也可以，不过需要相应的修改代码，请自行修改


- ExceptionMiddleware会捕获所有后端程序中未被捕获的异常，然后给前端发送一定的响应


3. 本次提交在前端services/http.js中添加了请求拦截器和响应拦截器
请求拦截器会给所有需要登录才能访问的请求自动添加一个请求头(具体实现方式是在前端的api上添加{withToken:true}字段)，以适应后端的JwtAuthenticationMiddleware；响应拦截器则会统一处理后端响应中的异常状态

示例写法：
```
export const login = (data) => http.post('/login', data)
export const logout = () => http.post('/logout',null,{withToken:true})
```
不需要登录的正常写，需要登陆的写上{withToken:true}

> highlight：2、3两点需要重点关注，否则可能会使你写的代码的演示效果不符合预期

4. 目前的登录只实现了Reader的登录，管理员账号目前是无法登录的

5. 在后端代码中提供了 backend\Services\Web\SecurityService.cs，大家可以通过注入的方式使用其中的函数来获取当前登录用户及其相关属性，比如ID,username等

6. 在前端代码中添加了store/user.js,大家可以使用它来全局获取当前用户信息

7. 新增了头像上传功能，自定义头像会上传至backend/wwwroot/avatars中存储