# File Transfer Protocol (FTP) Server

## Features
- Plain FTP
- Explicit FTP over TLS
- Implicit FTP over TLS
- IPS (Intrusion Prevention System) *IpsBlockTime based on MaxConnectionsPerMinute and MaxWrongPasswords*
- Supported FTP commands
    - APPE,
    - AUTH
    - CDUP
    - CWD
    - CD
    - DELE
    - EPSV
    - FEAT
    - HELP
    - LIST
    - MDTM
    - MKD
    - MLSD
    - MLST
    - NLST
    - NOOP
    - PASS
    - PASV
    - PBSZ
    - PORT
    - PROT
    - PWD
    - QUIT
    - REST
    - RETR
    - RMD
    - RNFR
    - RNTO
    - SIZE
    - STOR
    - SYST
    - TYPE
    - USER
    - XCUP
    - XCWD
    - XMKD
    - XPWD
    - XRMD

## Give a Star! :star:

If you like or are using this project to start your solution, please give it a star. Thanks!

## Nuget

[flexsys.TinyCLR.Networking.FTP.Server](https://www.nuget.org/packages/flexsys.TinyCLR.Networking.FTP.Server/)

## Requirements

**Software:** [Microsoft Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) and [GHI Electronics TinyCLR OS 2.0 or higher](https://www.ghielectronics.com/)

**Hardware:** project tested using [SCM20260D Dev](https://www.ghielectronics.com/sitcore/dev/)

**References**
- flexsys.TinyCLR.Core
- GHIElectronics.TinyCLR.Cryptography
- GHIElectronics.TinyCLR.Devices.Storage
- GHIElectronics.TinyCLR.IO
- GHIElectronics.TinyCLR.Native
- GHIElectronics.TinyCLR.Networking

## Example

```CSharp
using System.Security.Cryptography.X509Certificates;
using flexsys.TinyCLR.Core;
using Service = flexsys.TinyCLR.Service;
using FTPServer = flexsys.TinyCLR.Networking.FTP.Server;

static void Main()
{
    // network, time and storage initialization not included

    Service.FTP FTP = new Service.FTP(new FTPServer.Configuration()
    {
        AnonymousAuthentication = true,
        DocumentRoot = *.Drive.Name
    });
    FTP.Start();
}
```

### Authentication

```CSharp
FTPServer.Configuration
    AuthMethod = FTP_Authentication

public static void FTP_Authentication(object sender, FTPServer.AuthEventArgs e)
{
    e.IsAuthenticated = e.Username == "User" && e.Password == "Pass";
}
```

### TLS

```CSharp
byte[] servercert = Resources.GetBytes(Resources.BinaryResources.servercert);

X509Certificate certificate = new X509Certificate(servercert)
{
    PrivateKey = Resources.GetBytes(Resources.BinaryResources.serverkey),
};

FTPServer.Configuration
    Method = FTPServer.Method.Implicit|Explicit,
    Certificate = certificate
```

## RFC (Request for Comments)
- [RFC 959 - FILE TRANSFER PROTOCOL (FTP)](http://www.ietf.org/rfc/rfc959.txt)
- [RFC 1579 - Firewall-Friendly FTP](http://tools.ietf.org/rfc/rfc1579.txt)
- [RFC 2228 - FTP Security Extensions](http://tools.ietf.org/rfc/rfc2228.txt)
- [RFC 2389 - Feature negotiation mechanism for the File Transfer Protocol](http://tools.ietf.org/rfc/rfc2389.txt)
- [RFC 2428 - FTP Extensions for IPv6 and NATs](http://tools.ietf.org/rfc/rfc2428.txt)
- [RFC 2640 - Internationalization of the File Transfer Protocol](http://tools.ietf.org/rfc/rfc2640.txt)
- [RFC 3659 - Extensions to FTP](http://www.ietf.org/rfc/rfc3659.txt)
- [RFC 4217 - Securing FTP with TLS](http://www.ietf.org/rfc/rfc4217.txt)
- [RFC 5797 - FTP Command and Extension Registry](http://www.ietf.org/rfc/rfc4217.txt)

## Contributions

Contributions to this project are always welcome. Please consider forking this project on GitHub and sending a pull request to get your improvements added to the original project.

## Disclaimer

All source, documentation, instructions and products of this project are provided as-is without warranty. No liability is accepted for any damages, data loss or costs incurred by its use.
