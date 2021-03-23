# Authentication

## Features
- TOTP: Time-Based One-Time Password Algorithm

## Give a Star! :star:

If you like or are using this project to start your solution, please give it a star. Thanks!

## Nuget

[flexsys.TinyCLR.Authentication](https://www.nuget.org/packages/flexsys.TinyCLR.Authentication/)

## Requirements

**Software:** [Microsoft Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) and [GHI Electronics TinyCLR OS 2.0 or higher](https://www.ghielectronics.com/)

**Hardware:** project tested using [SCM20260D Dev](https://www.ghielectronics.com/sitcore/dev/)

**References**
- flexsys.TinyCLR.Cryptography
- GHIElectronics.TinyCLR.Drawing
- GHIElectronics.TinyCLR.Drivers.Barcode
- GHIElectronics.TinyCLR.Native

## Example

```CSharp
using System;
using System.Drawing;
using flexsys.TinyCLR.Authentication;

static void Main()
{
    // time and display initialization not included

    // at this point, your system must have correct time settings of both DateTime.Now and DateTime.UtcNow

    OTP otp = new OTP()
    {
        Key = "YOURSECRETKEY"
    };

    string code = otp.TOTP;
}
```

### Code validation

```CSharp
bool valid = otp.IsValid(CODE);
```

### QRCode generation

```CSharp
Bitmap qrcode = otp.QRCode("LABEL");
```

## RFC (Request for Comments)
- [RFC4226 - HOTP: An HMAC-Based One-Time Password Algorithm](http://www.ietf.org/rfc/rfc4226.txt)
- [RFC6238 - TOTP: Time-Based One-Time Password Algorithm](http://tools.ietf.org/rfc/rfc6238.txt)

## Contributions

Contributions to this project are always welcome. Please consider forking this project on GitHub and sending a pull request to get your improvements added to the original project.

## Disclaimer

All source, documentation, instructions and products of this project are provided as-is without warranty. No liability is accepted for any damages, data loss or costs incurred by its use.
