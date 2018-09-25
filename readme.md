# kasthack.roi

[![Nuget](https://img.shields.io/nuget/v/kasthack.roi.svg)](https://www.nuget.org/packages/kasthack.roi/)
[![NuGet](https://img.shields.io/nuget/dt/kasthack.roi.svg)](https://www.nuget.org/packages/kasthack.roi/)
[![Build status](https://img.shields.io/appveyor/ci/kasthack/kasthack-roi/master.svg)](https://ci.appveyor.com/project/kasthack/kasthack-roi)
[![license](https://img.shields.io/github/license/kasthack/kasthack.roi.svg)](LICENSE)
[![Gitter](https://img.shields.io/gitter/room/kasthack_roi/Lobby.svg)](https://gitter.im/kasthack-roi/Lobby)

## What

roi.ru API client

## Installation

`Install-Package kasthack.roi`

## Usage

```csharp
var client = new Client();

var active = await client.Poll().ConfigureAwait(false);

var petition = await client.Petition(active[759].Id).ConfigureAwait(false);
```

## Bugs / issues

Fork off / pull
