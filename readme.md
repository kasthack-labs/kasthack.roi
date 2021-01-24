# kasthack.roi

[![Nuget](https://img.shields.io/nuget/v/kasthack.roi.svg)](https://www.nuget.org/packages/kasthack.roi/)
[![NuGet](https://img.shields.io/nuget/dt/kasthack.roi.svg)](https://www.nuget.org/packages/kasthack.roi/)
[![.NET Status](https://github.com/kasthack-archive/kasthack.timelapser/workflows/.NET/badge.svg)](https://github.com/kasthack-archive/kasthack.timelapser/actions?query=workflow%3A.NET)
[![CodeQL](https://github.com/kasthack-archive/kasthack.timelapser/workflows/CodeQL/badge.svg)](https://github.com/kasthack-archive/kasthack.timelapser/actions?query=workflow%3ACodeQL)
[![Patreon pledges](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3Dkasthack%26type%3Dpledges&style=flat)](https://patreon.com/kasthack)
[![Patreon patrons](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3Dkasthack%26type%3Dpatrons&style=flat)](https://patreon.com/kasthack)

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
