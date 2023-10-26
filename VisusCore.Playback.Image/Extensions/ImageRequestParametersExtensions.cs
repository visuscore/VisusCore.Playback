using System;
using VisusCore.Playback.Image.Models;

namespace VisusCore.Playback.Image.Extensions;

public static class ImageRequestParametersExtensions
{
    public static bool HasCrop(this ImageTransformationsParameters parameters)
    {
        if (parameters is null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        return parameters.CropLeft is not null
            && parameters.CropTop is not null
            && parameters.CropRight is not null
            && parameters.CropBottom is not null;
    }
}
