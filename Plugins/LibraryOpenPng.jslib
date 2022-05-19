var LibraryOpenPng = {
    OpenPngAtNewTab: function (base64)
    {
        var url = 'data:application/png;base64,' + Pointer_stringify(base64);
        window.open(url);
    },
};
mergeInto(LibraryManager.library, LibraryOpenPng);