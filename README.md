![Logo](websocket-sharp_logo.png)

## Welcome to websocket-sharp! ##

**websocket-sharp** supports:

- **[RFC 6455](#supported-websocket-specifications)**
- **[WebSocket Client](#websocket-client)** and **[Server](#websocket-server)**
- **[Per-message Compression](#per-message-compression)** extension
- **[Secure Connection](#secure-connection)**
- **[HTTP Authentication](#http-authentication)**
- **[Query String, Origin header and Cookies](#query-string-origin-header-and-cookies)**
- **[Connecting through the HTTP Proxy server](#connecting-through-the-http-proxy-server)**
- .NET **4.5** or later (includes compatible)

## Build ##

websocket-sharp is built as a single assembly, **websocket-sharp.dll**.

websocket-sharp is developed with **[MonoDevelop]**. So the simple way to build is to open **websocket-sharp.sln** and run build for **websocket-sharp project** with any of the build configurations (e.g. `Debug`) in MonoDevelop.

## Install ##

### Self Build ###

You should add your **websocket-sharp.dll** (e.g. `/path/to/websocket-sharp/bin/Debug/websocket-sharp.dll`) to the library references of your project.

### NuGet Gallery ###

websocket-sharp is available on the **[NuGet Gallery]**.

- **[NuGet Gallery: websocket-sharp]**

You can add websocket-sharp to your project using the **NuGet Package Manager**, the following command in the **Package Manager Console**.

    PM> Install-Package WebSocketSharp.clone

## Usage ##

### WebSocket Client ###

```cs
using System;
using WebSocketSharp;

namespace Example
{
  public class Program
  {
    public static void Main (string[] args)
    {
      using (var ws = new WebSocket ("ws://dragonsnest.far/Laputa")) {
        ws.OnMessage = e => {
          Console.WriteLine ("Laputa says: " + e.Data);
          }

        ws.Connect ();
        ws.Send ("BALUS");
        Console.ReadKey (true);
      }
    }
  }
}
```

#### Step 1 ####

Required namespace.

```cs
using WebSocketSharp;
```

The `WebSocket` class exists in the `WebSocketSharp` namespace.

#### Step 2 ####

Creating a new instance of the `WebSocket` class with the WebSocket URL to connect.

```csharp
using (var ws = new WebSocket ("ws://example.com")) {
  ...
}
```

The `WebSocket` class inherits the `System.IDisposable` interface, so you can use the `using` statement. And the WebSocket connection will be closed with close status `1001` (going away) when the control leaves the `using` block.

#### Step 3 ####

Setting the `WebSocket` events.

##### WebSocket.OnOpen Event #####

A `WebSocket.OnOpen` event occurs when the WebSocket connection has been established.

```cs
ws.OnOpen = (sender, e) => {
  ...
};
```

`e` has passed as the `System.EventArgs.Empty`, so you don't use `e`.

##### WebSocket.OnMessage Event #####

A `WebSocket.OnMessage` event occurs when the `WebSocket` receives a message.

```cs
ws.OnMessage += (sender, e) => {
  ...
};
```

`e` has passed as a `WebSocketSharp.MessageEventArgs`.

`e.Type` property returns either `WebSocketSharp.Opcode.Text` or `WebSocketSharp.Opcode.Binary` that represents the type of the message. So by checking it, you can determine which item you should use.

If it returns `Opcode.Text`, you should use `e.Data` property that returns a `string` (represents the **Text** message).

Or if it returns `Opcode.Binary`, you should use `e.RawData` property that returns a `byte[]` (represents the **Binary** message).

```cs
if (e.Type == Opcode.Text) {
  // Do something with e.Data.
  ...

  return;
}

if (e.Type == Opcode.Binary) {
  // Do something with e.RawData.
  ...

  return;
}
```

##### WebSocket.OnError Event #####

A `WebSocket.OnError` event occurs when the `WebSocket` gets an error.

```cs
ws.OnError += (sender, e) => {
  ...
};
```

`e` has passed as a `WebSocketSharp.ErrorEventArgs`.

`e.Message` property returns a `string` that represents the error message. So you should use it to get the error message.

And if the error is due to an exception, you can get the `System.Exception` instance that caused the error, by using `e.Exception` property.

##### WebSocket.OnClose Event #####

A `WebSocket.OnClose` event occurs when the WebSocket connection has been closed.

```cs
ws.OnClose += (sender, e) => {
  ...
};
```

`e` has passed as a `WebSocketSharp.CloseEventArgs`.

`e.Code` property returns a `ushort` that represents the status code indicating the reason for the close, and `e.Reason` property returns a `string` that represents the reason for the close. So you should use them to get the reason for the close.

#### Step 4 ####

Connecting to the WebSocket server.

```cs
ws.Connect ();
```

If you would like to connect to the server asynchronously, you should use the `WebSocket.ConnectAsync ()` method.

#### Step 5 ####

Sending a data to the WebSocket server.

```cs
ws.Send (data);
```

The `WebSocket.Send` method is overloaded.

You can use the `WebSocket.Send (string)`, `WebSocket.Send (byte[])`, or `WebSocket.Send (System.IO.FileInfo)` method to send a data.

If you would like to send a data asynchronously, you should use the `WebSocket.SendAsync` method.

```cs
ws.SendAsync (data, completed);
```

And also if you would like to do something when the send is complete, you should set `completed` to any `Action<bool>` delegate.

#### Step 6 ####

Closing the WebSocket connection.

```cs
ws.Close (code, reason);
```

If you would like to close the connection explicitly, you should use the `WebSocket.Close` method.

The `WebSocket.Close` method is overloaded.

You can use the `WebSocket.Close ()`, `WebSocket.Close (ushort)`, `WebSocket.Close (WebSocketSharp.CloseStatusCode)`, `WebSocket.Close (ushort, string)`, or `WebSocket.Close (WebSocketSharp.CloseStatusCode, string)` method to close the connection.

If you would like to close the connection asynchronously, you should use the `WebSocket.CloseAsync` method.

### WebSocket Server ###

```cs
using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Example
{
  public class Laputa : WebSocketBehavior
  {
    protected override void OnMessage (MessageEventArgs e)
    {
      var msg = e.Data == "BALUS"
                ? "I've been balused already..."
                : "I'm not available now.";

      Send (msg);
    }
  }

  public class Program
  {
    public static void Main (string[] args)
    {
      var wssv = new WebSocketServer ("ws://dragonsnest.far");
      wssv.AddWebSocketService<Laputa> ("/Laputa");
      wssv.Start ();
      Console.ReadKey (true);
      wssv.Stop ();
    }
  }
}
```

#### Step 1 ####

Required namespace.

```cs
using WebSocketSharp.Server;
```

The `WebSocketBehavior` and `WebSocketServer` classes exist in the `WebSocketSharp.Server` namespace.

#### Step 2 ####

Creating the class that inherits the `WebSocketBehavior` class.

For example, if you would like to provide an echo service,

```cs
using System;
using WebSocketSharp;
using WebSocketSharp.Server;

public class Echo : WebSocketBehavior
{
  protected override void OnMessage (MessageEventArgs e)
  {
    Send (e.Data);
  }
}
```

And if you would like to provide a chat service,

```cs
using System;
using WebSocketSharp;
using WebSocketSharp.Server;

public class Chat : WebSocketBehavior
{
  private string _suffix;

  public Chat ()
    : this (null)
  {
  }

  public Chat (string suffix)
  {
    _suffix = suffix ?? String.Empty;
  }

  protected override void OnMessage (MessageEventArgs e)
  {
    Sessions.Broadcast (e.Data + _suffix);
  }
}
```

You can define the behavior of any WebSocket service by creating the class that inherits the `WebSocketBehavior` class.

If you override the `WebSocketBehavior.OnMessage (MessageEventArgs)` method, it's called when the `WebSocket` used in the current session in the service receives a message.

And if you override the `WebSocketBehavior.OnOpen ()`, `WebSocketBehavior.OnError (ErrorEventArgs)`, and `WebSocketBehavior.OnClose (CloseEventArgs)` methods, each of them is called when each event of the `WebSocket` (the `OnOpen`, `OnError`, and `OnClose` events) occurs.

The `WebSocketBehavior.Send` method sends a data to the client on the current session in the service.

If you would like to access the sessions in the service, you should use the `WebSocketBehavior.Sessions` property (returns a `WebSocketSharp.Server.WebSocketSessionManager`).

The `WebSocketBehavior.Sessions.Broadcast` method broadcasts a data to every client in the service.

#### Step 3 ####

Creating an instance of the `WebSocketServer` class.

```cs
var wssv = new WebSocketServer (4649);
wssv.AddWebSocketService<Echo> ("/Echo");
wssv.AddWebSocketService<Chat> ("/Chat");
wssv.AddWebSocketService<Chat> ("/ChatWithNyan", () => new Chat (" Nyan!"));
```

You can add any WebSocket service to your `WebSocketServer` with the specified behavior and path to the service, using the `WebSocketServer.AddWebSocketService<TBehaviorWithNew> (string)` or `WebSocketServer.AddWebSocketService<TBehavior> (string, Func<TBehavior>)` method.

The type of `TBehaviorWithNew` must inherit the `WebSocketBehavior` class, and must have a public parameterless constructor.

And also the type of `TBehavior` must inherit the `WebSocketBehavior` class.

So you can use the classes created in **Step 2** to add the service.

If you create an instance of the `WebSocketServer` class without a port number, the `WebSocketServer` set the port number to **80** automatically. So it's necessary to run with root permission.

    $ sudo mono example2.exe

#### Step 4 ####

Starting the WebSocket server.

```cs
wssv.Start ();
```

#### Step 5 ####

Stopping the WebSocket server.

```cs
wssv.Stop (code, reason);
```

The `WebSocketServer.Stop` method is overloaded.

You can use the `WebSocketServer.Stop ()`, `WebSocketServer.Stop (ushort, string)`, or `WebSocketServer.Stop (WebSocketSharp.CloseStatusCode, string)` method to stop the server.

### WebSocket Extensions ###

#### Per-message Compression ####

websocket-sharp supports the **[Per-message Compression][compression]** extension. (But it doesn't support with the [extension parameters].)

If you would like to enable this extension as a WebSocket client, you should set such as the following.

```cs
ws.Compression = CompressionMethod.Deflate;
```

And then your client sends the following header with the connection request to the server.

    Sec-WebSocket-Extensions: permessage-deflate

If the server supports this extension, it returns the same header. And when your client receives that header, it enables this extension.

### Secure Connection ###

websocket-sharp supports the **Secure Connection** with **SSL/TLS**.

As a **WebSocket Client**, you should create an instance of the `WebSocket` class with the **wss** scheme WebSocket URL.

```cs
using (var ws = new WebSocket ("wss://example.com")) {
  ...
}
```

And if you would like to use the custom validation for the server certificate, you should set the `WebSocket.SslConfiguration.ServerCertificateValidationCallback` property.

```cs
ws.SslConfiguration.ServerCertificateValidationCallback =
  (sender, certificate, chain, sslPolicyErrors) => {
    // Do something to validate the server certificate.
    ...

    return true; // If the server certificate is valid.
  };
```

If you set this property to nothing, the validation does nothing with the server certificate, and returns `true`.

As a **WebSocket Server**, you should create an instance of the `WebSocketServer` or `HttpServer` class with some settings for secure connection, such as the following.

```cs
var wssv = new WebSocketServer (4649, true);
wssv.SslConfiguration.ServerCertificate =
  new X509Certificate2 ("/path/to/cert.pfx", "password for cert.pfx");
```

### HTTP Authentication ###

websocket-sharp supports the **[HTTP Authentication (Basic/Digest)][rfc2617]**.

As a **WebSocket Client**, you should set a pair of user name and password for the HTTP authentication, using the `WebSocket.SetCredentials (string, string, bool)` method before connecting.

```cs
ws.SetCredentials ("nobita", "password", preAuth);
```

If `preAuth` is `true`, the `WebSocket` sends the Basic authentication credentials with the first connection request to the server.

Or if `preAuth` is `false`, the `WebSocket` sends either the Basic or Digest (determined by the unauthorized response to the first connection request) authentication credentials with the second connection request to the server.

As a **WebSocket Server**, you should set an HTTP authentication scheme, a realm, and any function to find the user credentials before starting, such as the following.

```cs
wssv.AuthenticationSchemes = AuthenticationSchemes.Basic;
wssv.Realm = "WebSocket Test";
wssv.UserCredentialsFinder = id => {
  var name = id.Name;

  // Return user name, password, and roles.
  return name == "nobita"
         ? new NetworkCredential (name, "password", "gunfighter")
         : null; // If the user credentials aren't found.
};
```

If you would like to provide the Digest authentication, you should set such as the following.

```cs
wssv.AuthenticationSchemes = AuthenticationSchemes.Digest;
```

### Query String, Origin header and Cookies ###

As a **WebSocket Client**, if you would like to send the **Query String** with the WebSocket connection request to the server, you should create an instance of the `WebSocket` class with the WebSocket URL that includes the [Query] string parameters.

```cs
using (var ws = new WebSocket ("ws://example.com/?name=nobita")) {
  ...
}
```

And if you would like to send the **Origin header** with the WebSocket connection request to the server, you should set the `WebSocket.Origin` property to an allowable value as the [Origin header] before connecting, such as the following.

```cs
ws.Origin = "http://example.com";
```

And if you would like to send the **Cookies** with the WebSocket connection request to the server, you should set any cookie using the `WebSocket.SetCookie (WebSocketSharp.Net.Cookie)` method before connecting, such as the following.

```cs
ws.SetCookie (new Cookie ("name", "nobita"));
```

As a **WebSocket Server**, if you would like to get the **Query String** included in each WebSocket connection request, you should access the `WebSocketBehavior.Context.QueryString` property, such as the following.

```cs
public class Chat : WebSocketBehavior
{
  private string _name;
  ...

  protected override void OnOpen ()
  {
    _name = Context.QueryString["name"];
  }

  ...
}
```

And if you would like to validate the **Origin header**, **Cookies**, or both included in each WebSocket connection request, you should set each validation with your `WebSocketBehavior`, for example, using the `AddWebSocketService<TBehavior> (string, Func<TBehavior>)` method with initializing, such as the following.

```cs
wssv.AddWebSocketService<Chat> (
  "/Chat",
  () => new Chat () {
    OriginValidator = val => {
      // Check the value of the Origin header, and return true if valid.
      Uri origin;
      return !val.IsNullOrEmpty () &&
             Uri.TryCreate (val, UriKind.Absolute, out origin) &&
             origin.Host == "example.com";
    },
    CookiesValidator = (req, res) => {
      // Check the Cookies in 'req', and set the Cookies to send to the client with 'res'
      // if necessary.
      foreach (Cookie cookie in req) {
        cookie.Expired = true;
        res.Add (cookie);
      }

      return true; // If valid.
    }
  });
```

Also, if you would like to get each value of the Origin header and cookies, you should access each of the `WebSocketBehavior.Context.Origin` and `WebSocketBehavior.Context.CookieCollection` properties.

### Connecting through the HTTP Proxy server ###

websocket-sharp supports to connect through the **HTTP Proxy** server.

If you would like to connect to a WebSocket server through the HTTP Proxy server, you should set the proxy server URL, and if necessary, a pair of user name and password for the proxy server authentication (Basic/Digest), using the `WebSocket.SetProxy (string, string, string)` method before connecting.

```cs
var ws = new WebSocket ("ws://example.com");
ws.SetProxy ("http://localhost:3128", "nobita", "password");
```

I tested this with the [Squid]. And it's necessary to disable the following configuration option in **squid.conf** (e.g. `/etc/squid/squid.conf`).

```
# Deny CONNECT to other than SSL ports
#http_access deny CONNECT !SSL_ports
```

## Supported WebSocket Specifications ##

websocket-sharp supports **[RFC 6455][rfc6455]**, and it's based on the following WebSocket references:

- **[The WebSocket Protocol][rfc6455]**
- **[The WebSocket API][api]**
- **[Compression Extensions for WebSocket][compression]**

## License ##

websocket-sharp is provided under **[The MIT License]**.


[Audio Data delivery server]: http://agektmr.node-ninja.com:3000
[Echo server]: http://www.websocket.org/echo.html
[Example]: https://github.com/sta/websocket-sharp/tree/master/Example
[Example1]: https://github.com/sta/websocket-sharp/tree/master/Example1
[Example2]: https://github.com/sta/websocket-sharp/tree/master/Example2
[Example3]: https://github.com/sta/websocket-sharp/tree/master/Example3
[Json.NET]: http://james.newtonking.com/projects/json-net.aspx
[Mono]: http://www.mono-project.com
[MonoDevelop]: http://monodevelop.com
[NuGet Gallery]: http://www.nuget.org
[NuGet Gallery: websocket-sharp]: http://www.nuget.org/packages/WebSocketSharp
[Origin header]: http://tools.ietf.org/html/rfc6454#section-7
[Query]: http://tools.ietf.org/html/rfc3986#section-3.4
[Security Sandbox of the Webplayer]: http://docs.unity3d.com/Manual/SecuritySandbox.html
[Squid]: http://www.squid-cache.org
[The MIT License]: https://raw.github.com/sta/websocket-sharp/master/LICENSE.txt
[Unity]: http://unity3d.com
[Unity Licenses Comparison]: http://unity3d.com/unity/licenses
[WebSocket-Sharp for Unity]: http://u3d.as/content/sta-blockhead/websocket-sharp-for-unity
[api]: http://www.w3.org/TR/websockets
[api_ja]: http://www.hcn.zaq.ne.jp/___/WEB/WebSocket-ja.html
[compression]: http://tools.ietf.org/html/draft-ietf-hybi-permessage-compression-09
[draft-hixie-thewebsocketprotocol-75]: http://tools.ietf.org/html/draft-hixie-thewebsocketprotocol-75
[draft-ietf-hybi-thewebsocketprotocol-00]: http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-00
[draft75]: https://github.com/sta/websocket-sharp/tree/draft75
[extension parameters]: http://tools.ietf.org/html/draft-ietf-hybi-permessage-compression-09#section-8.1
[hybi-00]: https://github.com/sta/websocket-sharp/tree/hybi-00
[master]: https://github.com/sta/websocket-sharp/tree/master
[rfc2617]: http://tools.ietf.org/html/rfc2617
[rfc6455]: http://tools.ietf.org/html/rfc6455
[rfc6455_ja]: http://www.hcn.zaq.ne.jp/___/WEB/RFC6455-ja.html
