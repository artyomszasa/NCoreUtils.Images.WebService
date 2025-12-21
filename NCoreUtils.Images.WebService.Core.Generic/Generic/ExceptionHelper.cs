using System;
using NCoreUtils.Images.WebService;

namespace NCoreUtils.Images.Generic;

public static class ExceptionHelper
{
    public static ImageErrorData GetErrorData(ImageException exn)
        => exn switch
        {
            UnsupportedResizeModeException e => new UnsupportedResizeModeData(e.ErrorCode, e.Message, e.ResizeMode, e.Width, e.Height),
            UnsupportedImageTypeException e => new UnsupportedImageTypeData(e.ErrorCode, e.Message, e.ImageType),
            InternalImageException e => new InternalImageErrorData(e.ErrorCode, e.Message, e.InternalCode),
            ImageException e => new ImageErrorData(e.ErrorCode, e.Message)
        };

    public static ImageErrorData GetErrorData(Exception exn) => exn switch
    {
        ImageException vexn => GetErrorData(vexn),
        _ => new ImageErrorData(ErrorCodes.GenericError, exn.Message)
    };
}