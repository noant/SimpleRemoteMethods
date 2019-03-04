# SimpleRemoteMethods
A project that allows to easily create a secure and cross-platform client-server part of project. Based on protobuf serialization and HttpListener.

#### Start server
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
  
  #### Contracts
  
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

#### Client class generation

Use srmGen.exe to generate client class.

```
srmGen.exe {path to contracts interface library} {namespace and class of target contracts interface} {result class namespace} {result class name} {generated file path}
```

Example how it looks in Lazurite.MainDomain in build events:

```
srmGen.exe bin\$(ConfigurationName)\netstandard2.0\Lazurite.MainDomain.dll Lazurite.MainDomain.IServer Lazurite.MainDomain LazuriteClient ..\Lazurite.MainDomain\LazuriteClient.cs
```

Generated class example: [Lazurite.MainDomain.LazuriteClient.cs](https://github.com/noant/Lazurite/blob/master/Lazurite/Lazurite.MainDomain/LazuriteClient.cs)

#### Prepare Windows for server

In Windows, you need to perform a lot of actions to start the server: add a rule for a firewall, reserve an address, bind a certificate to a port. SimpleRemoteMethods has a library to do this automatically. Add SimpleRemoteMethods.Utils.Windows.dll and run PrepareHttpServer<T> or PrepareHttpsServer<T> method after creating the server. Example:

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


#### Authentication

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

#### Token distribution

Instead of passing user/password every time remote method called, the user token is transferred. Token distribution proceed automatically.

Change token lifetime example:
```csharp
server.TokenDistributor.TokenLifetime = TimeSpan.FromHours(24);
```
Revoke user token (when password/login changed, etc):

```csharp
server.TokenDistributor.RevokeToken("someuser");
```

To all other, there is a possibile to override standard token distributor by creating class< derived from [ITokenDistributor](https://github.com/noant/SimpleRemoteMethods/blob/master/SimpleRemoteMethods.ServerSide/ITokenDistributor.cs)

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
