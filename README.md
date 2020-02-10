# gRPC
## Why gRPC
Firstly, gRPC is based on HTTP/2. Thus, we might first say what are the key features of HTTP/2
- Binary protocol
- Streams
- Request multiplexing over single TCP connection

Now we can say on top of HTTP/2 features, gRPC provides:
- Performance: coming from binary protocol and multiplexing
- Interoperability
- Streaming
- Deadline/timeout and cancellation: Both client and server can define a timeout. Furthermore, client can abort an operation earlier if necessary. 
- Security

## Creating the Server
Dotnet Core and Visual Studio both have a template to create a gRPC server, but in this tutorial, I am going to create it from scratch. In addition, we are going to create the client.

```powershell
md gRPC
cd .\gRPC\

dotnet new web -o Server
cd .\Server\
dotnet build 
dotnet add .\Server.csproj package Grpc.AspNetCore
```
Note: The reason for making a build at this stage is to have the bin\Debug\netcoreapp3.1 folder created which would be used by the *protoc*.

Open the project folder in Code. In *ConfigureServices* method add 
```csharp
            services.AddGrpc();
```

In *Configure* method, in *UseEndpoints* block add:
```csharp
            endpoints.MapGrpcService<CalcService>();

```
Now we need to add the *CalcService*. We can create a folder called *Services* and put our CalcService there.
For now it can be just an empty class.

### Adding protos
Add a *Protos* folder and create a "calc.proto" inside that.

Edit the *csproj* file and add:
```xml
  <ItemGroup>
    <Protobuf Include="Protos\calc.proto" GrpcServices="Server" />
  </ItemGroup>
```

The content of *proto* would be:
```proto
syntax = "proto3";

option csharp_namespace = "Server";

package calc;

service Calc {
  rpc Add (AddRequest) returns (AddResponse);
}

message AddRequest {
    int32 a = 1;
    int32 b = 2;
}

message AddResponse {
    int32 c = 1;
}
```
Note 1: The first line is mandatory.

Note 2: the namespace would be used in the generated C# code. 

Note 3: the values in front of the a, b and c are their order not their default. They should be positive. The reason is *proto* format, unlike XML and json doesn't serialize the property names and instead works based on the data in order.

### Compiling protos
You need to first Download the *protoc* for your operating system from their [release repository](https://github.com/protocolbuffers/protobuf/releases).
After extracting you would find the *protoc* in the bin folder.  

```powershell
protoc.exe --csharp_out=.\bin\Debug\netcoreapp3.1 --csharp_opt=,base_namespace=Server .\Protos\calc.proto
dotnet build .
```

### Using the classes created by protoc
There would be a abstract partial class called *CalcBase* in *CalcGrp.cs*. The *CalcService* would override the Add method of the *CalcBase*. The full code would be like:
```csharp
    public class CalcService : CalcBase {
        public async override Task<AddResponse> Add(AddRequest request, ServerCallContext context) 
        {
            return await Task.FromResult(new AddResponse
            {
                C = request.A + request.B
            });
        }
    }
```


## Client
If you are in the *Server* folder go one level up and start creating a console application

```powershell
dotnet new console -o Client
cd .\Client\
dotnet build
dotnet add .\Client.csproj package  Grpc.Net.Client
dotnet add .\Client.csproj package  Google.Protobuf
dotnet add .\Client.csproj package  Grpc.Tools
```
Note: Again, we have build the project early to have the bin\Debug\netcoreapp3.1 folder created so *protoc* tool can write its output there.

Now we need to copy the Protos folder and its contents from the server project:
```powershell
md Protos
Copy ..\Server\Protos\* .\Protos
```
Edit your *csproj* file and add:
```xml
  <ItemGroup>
    <Protobuf Include="Protos\calc.proto" GrpcServices="Client" />
  </ItemGroup>
```
Note: If you are copying from the server application, remember to change the *GrpcServices* to *Client*.
Edit the *proto* file and change the namespace to *Client*.
```proto
option csharp_namespace = "Client";
```

Now create the proxy files using *protoc*.
```powershell
protoc.exe --csharp_out=.\bin\Debug\netcoreapp3.1 --csharp_opt=,base_namespace=Client .\Protos\calc.proto

```
In the program edit the *main* method to:
```csharp
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5021");

            var calcClient = new Calc.CalcClient(channel);
            var addreply = await calcClient.AddAsync(
                new AddRequest { A = 2, B = 3 }
                );
            Console.WriteLine(addreply.C);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
```
This time we are using the partial class *CalcClient* nested in *Calc* class.

## Running
Remember before running the server you need to trust to the certificate:
```powershell
dotnet dev-certs https --trust
```
The code can be found in (github)[https://github.com/mkokabi/gRPC]


