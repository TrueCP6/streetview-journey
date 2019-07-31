# streetview-journey
For now the only way to use it is to clone, open the project in vs and use it how you like. I'm too lazy to make it into a library for now. If you have any suggestions or ideas make an issue or pull request.

### Things that can be done with it
[Playlist of videos](https://www.youtube.com/playlist?list=PL2gaTlKIJh0f7RqPazkAStxvLdSbFrrhy)

### Dependencies
Requires [geckodriver](https://github.com/mozilla/geckodriver/releases) and its path to be set as `Download.geckoDriverPath`. Requires an api and signing key (signing secret) for streetview static api. These must be set as `Web.apiKey` and `Web.signingKey`. Requires [Firefox](https://ninite.com/firefox/) to be installed in the default path.

### File types
GPX files can be downloaded from [here](https://mapstogpx.com/) using a google maps route link. This tool uses its own basic file type called .svj for storing of point data.

### Features
- `Web.GetGooglePanoID` Gets the ID of a panorama uploaded by google from a point. Returns `null` if one if not found.
- `Web.GetExact` Gets the point of the nearest panorama from a pano ID or point. Returns `(0, 0)` if none is found.
- `Web.AllUsable` Whether or not every point of the location data array has a panorama within the search radius.
- `Web.IsUsable` Whether a point has a panorama within the search radius.
- `Web.GetPanoID` Gets the ID of any panorama uploaded from a point. Returns `null` if one if not found.
- `Web.Sign` Signs a url for use with the static streetview image api.
- `Smart.ToSVJ` Takes a .gpx file and interpolates it into a fully detailed .svj file automatically.
- `Remove.Dupes` Removes all duplicates from a location data array.
- `Remove.Nulls` Removes all nulls from a string array.
- `Remove.Zeroes` Removes all `(0, 0)` points
- `Modify.ResizeImage` Returns a resized bitmap image.
- `Modify.SmoothTrim` Tries to trim down to a length while maintaining a constant speed.
- `Modify.Trim` Basic trim.
- `Modify.Interpolate` Takes an array of points and interpolates any additional panoramas found between into a new array.
- `Import.Auto` Automatically chooses which file type to import. Returns location data array.
- `Import.SVJ` Imports a .svj file into a location data array.
- `Import.GPX` Imports a .gpx file into a location data array.
- `Get.RandomGooglePanoID` Returns a random panorama uploaded by google from anywhere on earth.
- `Get.ThumbnailURL` Returns a url to a low resolution thumbnail of a panorama.
- `Get.UniquePanoIDs` Gets an array of random panorama IDs with a length of your choice
- `Get.RandomUsablePoint` Returns a random point at the location of a panorama on earth.
- `Get.TileURL` Gets the url of a tile for a streetview panorama.
- `Get.StreetviewURL` Gets a link to the streetview browser from a point.
- `Get.ImageURL` Gets the url to a static streetview image using the static streetview api.
- `Get.DistancesString` Gets a string of all the distances for a location data array.
- `Get.Distances` Gets an array of the distances between each point for a location data array.
- `Get.AverageDistance` The average distance between points for a location data array in meters.
- `Get.TotalDistance` The total distance for a location data array in meters.
- `Get.BearingsString` String of all the bearings of a bearings array.
- `Get.String` String of all points in a location data array.
- `Get.ExactCoords` Takes a location data array and snaps every point to the place of a panorama.
- `Get.PanoIDsString` String of all panorama IDs from a location data array.
- `Export.ToSVJ` Saves the given location data array as an svj file.
- `Download.AllPanoramas` Downloads a 360 equirectangular image to a folder for every point in a sequence.
- `Download.Panorama` Returns a bitmap in equirectangular format. Uses 400mb of memory per image so arrays are a bad idea. Will throw an exception if given a panorama ID uploaded by a user starting with `CAosSLEF`
- `Download.AllScreenshots` Opens a window of Firefox and takes and saves a screenshot for every point. Requires `Download.geckoDriverPath` to be set. Returns a string array of short road/place names the same length as the input location data array. Can still be used like a void method.
- `Download.AllImages` Uses the static streetview api to download an image for every point.
- `Calculate.Offset` Calculates the offset angle needed to offset a panorama using bearing.
- `Calculate.ZoomToFOV` Converts streetview zoom to fov.
- `Calculate.FOVtoZoom` Converts fov to streetview zoom level.
- `Calculate.AverageBearing` The average bearing of an array of bearings.
- `Calculate.Distance` Calculates the distance between 2 points in meters.
- `Calculate.Bearings` Calculates the bearing from one point to another.
- `Calculate.Midpoint` Midpoint between 2 points.
- `Calculate.AddBearing` Adds 2 bearings together with wrap around.
- `Bearing.OffsetPanorama` Returns an offset bitmap wrapped around to the right for a certain amount in degrees.
- `Bearing.Trim` Same function as `Modify.Trim`, instead for bearings.
- `Bearing.Smooth` Smoothes out a bearings array.
- `Bearing.Trackpoint` Returns a bearings array where the bearing tracks a specific point.
- `Bearing.Get` Gets the bearings for a location data array where the bearing for each point will always be the direction of the next point.
- `Modify.CropImage` Crops a bitmap to the size and position of a Rectangle object.
- `Smart.RandomImageFromRandomPanorama` Returns a 2D image from a random panorama facing a random direction. Zoom of image is higher the lower resolution you request.
- `Smart.ScreenshotSequence` Downloads a sequence of screenshots to a folder from an input file. You display scaling % must be taken into account for resolution.
- `Smart.PanoramaSequence` Downloads a sequence of panoramas to a folder from an input file. Panoramas have an aspect ratio of 2:1.
- `Smart.ImageSequence` Downloads a sequence of streetview images to a folder from the static streetview api.
- `Get.PanoIDs` Gets an array of panorama IDs from a location data array.
- `Get.GooglePanoIDs` Gets an array of panorama IDs uploaded by Google from a location data array.