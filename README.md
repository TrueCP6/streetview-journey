# StreetviewJourney
A library for modifying and using point, panorama and route data for Google Streetview.

## Example results
![Example Panorama](https://i.imgur.com/VF3rQgT.jpg)
### [YouTube Playlist](https://www.youtube.com/playlist?list=PL2gaTlKIJh0f7RqPazkAStxvLdSbFrrhy)

# Usage

## Routes
`Route`s can be created from [.gpx](https://mapstogpx.com/) or [.svj](https://github.com/TrueCP6/streetview-journey/tree/master/Example%20SVJs) files, `Point` arrays or `PanoID` arrays.
`Route`s can be modified with various methods this library contains.
`Route`s can be downloaded as `ImageFolder` image sequences.
Example usage:
```c#
Route route = new Route(@"D:\routes\old.gpx");
route = route.Interpolate(5).GetBearings().SmoothBearings();
Console.WriteLine(route.TotalDistance);
route.Save(@"D:\routes\new.svj");
```

## Points
`Point`s are latitude-longitude co-ordinates used to keep track of the positions of `Route`s and `PanoID`s which come with an attached `Bearing`.
They can be modified using various methods.
Example usage:
```c#
Point point1 = new Point(40.748705, -73.985571);
Point point2 = Point.RandomUsable();
point1 = point1.Exact();
Console.WriteLine(point1.DistanceTo(point2) + " metres");
```

## Bearings
`Bearing`s are used to store compass heading values from 0 to 360.
These are attached to a point and can be calculated using `Route` and `Point` methods.

## ImageFolders
These are used to manage downloaded images from `Route`s.
The `ImageFolder` can be deleted or the image sequence can be converted into a video using FFmpeg.

## PanoIDs
Panorama IDs are linked to specific panorama on Google Streetview and are split into two categories: first party and third party.
First party panoramas are uploaded by Google while third party panoramas are uploaded by users.
Certain methods can only be used with first party panoramas.
Example usage:
```c#
PanoID pano = PanoID.RandomUsable();
if (!pano.isThirdParty)
    pano.DownloadPanorama().Save(@"D:\images\panorama.jpg", ImageFormat.Jpeg);
Point point = pano.Position;
```

# Setup
`Setup.SetStaticStreetviewAPIInfo` must be executed to allow usage of most functionality within this library.
This requires a [Google Streetview Static API Key and URL Signing Secret](https://developers.google.com/maps/documentation/streetview/get-api-key) and billing to be enabled on the account.
`Setup.DontBillMe` can be set to prevent unintentional billing and allows only allows metadata queries to be made (which is not billed by Google).
`Setup.FFmpegExecutablesFolder` should be the directory containing the [FFmpeg](https://www.ffmpeg.org/download.html) executables.
`Setup.DownloadFFmpeg()` can however be used to download the latest version of FFmpeg.
`Setup.GeckodriverPath` should be the path to the [geckodriver](https://github.com/mozilla/geckodriver/releases) executable.
The latest version of geckodriver can be automatically downloaded using `Setup.DownloadGeckodriver()`.

# Miscellaneous Features
* Setting windows desktop wallpaper
* Signing Static Streetview API URLs