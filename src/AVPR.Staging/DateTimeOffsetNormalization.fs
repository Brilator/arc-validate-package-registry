namespace AVPR.Staging

open System
open System.Globalization

[<RequireQualifiedAccess>]
module DateTimeOffsetNormalization =

    let truncateToSeconds (date: DateTimeOffset) =
        DateTimeOffset.ParseExact(
            date.ToString("yyyy-MM-dd HH:mm:ss zzzz", CultureInfo.InvariantCulture),
            "yyyy-MM-dd HH:mm:ss zzzz",
            CultureInfo.InvariantCulture
        )
