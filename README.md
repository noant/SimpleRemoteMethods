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

