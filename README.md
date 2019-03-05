# SimpleRemoteMethods
A project that allows to easily create a secure and cross-platform client-server part of project. Based on protobuf serialization and HttpListener.

### Start server
Create and start server:

```csharp
var server = new Server<IContracts>(contractsObject, useHttps, serverPort, secretKey);
server.StartAsync();
```
  - [IContracts] is contracts interface.
  - [contractsObject] - a class object derived from a contract interface that contains target methods.
  - [useHttps] - boolean, use https.
  - [serverPort] - server port.
  - [secretKey] - key for intermediate AES encryption.
  
### Contracts
  
Example of contracts interface:
    
```csharp
public interface IContracts
{
    [Remote]
    void TestMethod1();
    
    [Remote]
    int TestMethod2(string str);
}
```

Contracts class example:

```csharp
public class Contracts: IContracts 
{
    public void TestMethod1()
    {
        Console.WriteLine("TestMethod1");
    }
    
    public int TestMethod2(string str)
    {
        return str.Length;
    }
}
```

### Client class generation

Use srmGen.exe to generate client class.

```
srmGen.exe {path to contracts interface library} {namespace and interface name of target contracts} {result class namespace} {result class name} {generated file path}
```

Example how it looks in Lazurite.MainDomain in build events:

```
srmGen.exe bin\$(ConfigurationName)\netstandard2.0\Lazurite.MainDomain.dll Lazurite.MainDomain.IServer Lazurite.MainDomain LazuriteClient ..\Lazurite.MainDomain\LazuriteClient.cs
```

Generated class example: [Lazurite.MainDomain.LazuriteClient.cs](https://github.com/noant/Lazurite/blob/master/Lazurite/Lazurite.MainDomain/LazuriteClient.cs)

### Prepare Windows for server

In Windows, you need to perform a lot of actions to start the server: add a rule for a firewall, reserve an address, bind a certificate to a port. SimpleRemoteMethods has a library to do this automatically. Add SimpleRemoteMethods.Utils.Windows.dll and run [PrepareHttpServer<T>] or [PrepareHttpsServer<T>] method after creating the server. Example:

```csharp
var server = new Server<IContracts>(contractsObject, true, serverPort, secretKey);
ServerHelper.PrepareHttpsServer(server, certificateHash, appUniqueId);
server.StartAsync();
```

```csharp
var server = new Server<IContracts>(contractsObject, false, serverPort, secretKey);
ServerHelper.PrepareHttpServer(server, appUniqueId);
server.StartAsync();
```

  - [certificateHash] - string, hash of target https certificate.
  - [appUniqueId] - string, custom id of current app, used for firewall rules naming.


### Authentication

Interface IAuthentication allows to override standard authentication stub. 

```csharp
public interface IAuthenticationValidator
{
    bool Authenticate(string userName, string password);
}
```
```csharp
public class MyCustomAuthentication
{
    public bool Authenticate(string userName, string password)
    {
        return userName == "someuser" && password = "password1";
    }
}
```
```csharp
var server = new Server<IContracts>(contractsObject, useHttps, serverPort, secretKey);
server.AuthenticationValidator = new MyCustomAuthentication();
server.StartAsync();
```

### Token distribution

Instead of passing user/password every time remote method called, the user token is transferred. Token distribution proceed automatically.

Change token lifetime example:
```csharp
server.TokenDistributor.TokenLifetime = TimeSpan.FromHours(24);
```
Revoke user token (when password/login changed, etc):

```csharp
server.TokenDistributor.RevokeToken("someuser");
```

To all other, there is a possibile to override standard token distributor by creating a class, derived from [ITokenDistributor](https://github.com/noant/SimpleRemoteMethods/blob/master/SimpleRemoteMethods.ServerSide/ITokenDistributor.cs):

```csharp    /// <summary>
/// Class that conains logic for user token distribution
/// </summary>
public interface ITokenDistributor
{
    /// <summary>
    /// Returns true if the token was created and it is still alive
    /// </summary>
    /// <param name="token"></param>
    /// <param name="tokenInfo">Info about token (user name, etc)</param>
    /// <returns></returns>
    bool Authenticate(string token, out TokenInfo tokenInfo);

    /// <summary>
    /// Creates new token for user/ip
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="clientIp"></param>
    /// <returns>token string</returns>
    string RequestToken(string userName, string clientIp);

    /// <summary>
    /// Cancel user token
    /// </summary>
    /// <param name="userName"></param>
    void RevokeToken(string userName);

    /// <summary>
    /// The time interval while the token is alive
    /// </summary>
    TimeSpan TokenLifetime { get; set; }
}
```
```csharp
server.TokenDistributor = new MyCustomTokenDistributor();
```

### Bruteforce checker

Simple bruteforce checker already exists in project, but user can override it by creating a class, derived from [IBruteforceChecker](https://github.com/noant/SimpleRemoteMethods/blob/master/SimpleRemoteMethods.ServerSide/IBruteforceChecker.cs):

```csharp
public interface IBruteforceChecker
{
    /// <summary>
    /// Check last login activity and decides whether
    /// the user is trying to bruteforce a password
    /// </summary>
    /// <param name="loginString">Is client user name or ip</param>
    /// <returns></returns>
    bool CheckIsBruteforce(string loginString);

    /// <summary>
    /// Check the user or ip is in wait list
    /// </summary>
    /// <param name="loginString">Is client user name or ip</param>
    /// <returns></returns>
    bool IsWaitListContains(string loginString);
}
```
```csharp
server.BruteforceCheckerByLogin = new MyCustomBruteforceChecker();
server.BruteforceCheckerByIpAddress = new MyCustomBruteforceChecker();
```

### Events

```csharp
/// <summary>
/// Raises when need to write log
/// </summary>
public event EventHandler<LogRecordEventArgs> LogRecord;

/// <summary>
/// Raises before server start
/// </summary>
public event EventHandler<EventArgs> BeforeServerStart;

/// <summary>
/// Raises after server started
/// </summary>
public event EventHandler<EventArgs> AfterServerStarted;

/// <summary>
/// Raises after server stopped
/// </summary>
public event EventHandler<EventArgs> AfterServerStopped;

/// <summary>
/// Access to http listener before server starts listen
/// </summary>
public event EventHandler<TaggedEventArgs<HttpListener>> HttpListenerCustomSetting;

/// <summary>
/// Access to http context on client connect
/// </summary>
public event EventHandler<TaggedEventArgs<HttpListenerContext>> HttpRequestCustomHandling;

/// <summary>
/// Access to request on client connect
/// </summary>
public event EventHandler<TaggedEventArgs<Request>> UserRequest;

/// <summary>
/// Access to response on user request
/// </summary>
public event EventHandler<TaggedEventArgs<Response>> ServerResponse;

/// <summary>
/// Access to user token request
/// </summary>
public event EventHandler<TaggedEventArgs<UserTokenRequest>> UserTokenRequest;

/// <summary>
/// Access to user token response
/// </summary>
public event EventHandler<TaggedEventArgs<UserTokenResponse>> ServerUserTokenResponse;

/// <summary>
/// Access to error response
/// </summary>
public event EventHandler<TaggedEventArgs<ErrorResponse>> ErrorServerResponse;

/// <summary>
/// Raises before method calls
/// </summary>
public event EventHandler<RequestEventArgs> MethodCall;
```

### MaxConcurrentCalls

If concurrent calls count exceeds the value of the MaxConcurrentCalls property, the server suspends receiving all incoming connections.

```csharp
server.MaxConcurrentCalls = 20; // Default value
```

### MaxMessageLength

Maximum length of request in bytes.

```csharp
server.MaxMessageLength = 20000; // Default value
```

### Current request context

Current request context allows you to determine current caller, user-ip and other useful information about the execution of the current method.

Example:

```csharp
public interface IContracts
{
    [Remote]
    void TestMethod1();
    
    [Remote]
    int TestMethod2(string str);
}
```

```csharp
public class Contracts: IContracts 
{
    private void WriteUserInfo()
    {
        var currentUser = Server<IContracts>.CurrentRequestContext.UserName;
        var currentUserIp = Server<IContracts>.CurrentRequestContext.ClientIp;        
        Console.WriteLine(currentUser + " " + currentUserIp);
    }

    public void TestMethod1()
    {
        WriteUserInfo();
    }
    
    public int TestMethod2(string str)
    {
        WriteUserInfo();
        return str.Length;
    }
}
```

### Exceptions handling

There is one class to determine which exception was thrown on the server: [RemoteException](https://github.com/noant/SimpleRemoteMethods/blob/master/SimpleRemoteMethods.Bases/RemoteException.cs).


```csharp
try
{
    await client.TestMethod1()
}
catch (RemoteException e) when (e.Code == ErrorCode.LoginOrPasswordInvalid)
{
    // Do something when user/password is invalid
}
catch (RemoteException e) when (e.Code == ErrorCode.UnknownData || e.Code == ErrorCode.DecryptionErrorCode)
{
    // Do something when secret key is invalid
}
catch (RemoteException e) when (e.Code == ErrorCode.BruteforceSuspicion)
{
    // Do something when there is bruteforce suspition
}
catch (RemoteException)
{
    // Other actions
}
catch (Exception)
{
    // Other actions
}
```

All error codes there: [ErrorCode](https://github.com/noant/SimpleRemoteMethods/blob/master/SimpleRemoteMethods.Bases/ErrorCode.cs).
