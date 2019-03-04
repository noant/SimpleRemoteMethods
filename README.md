# SimpleRemoteMethods
A project that allows to easily create a secure client-server part of project. Based on protobuf serialization and HttpListener.

### Server
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
public interface IContracts {
    [Remote]
    void TestMethod1();
    
    [Remote]
    void TestMethod2();
}
```
  
