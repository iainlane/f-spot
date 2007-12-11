#!/bin/sh
# Description:	Updates upstream source file to link with system Mono.Addins
# Author:	Mirco Bauer <meebey@debian.org>
# License:	GPL-2

echo "Updating Mono.Addins references"
find . \( -name "*.am" -or -name "*.in*" \) -exec perl -pe 's!-r\:\$\(DIR_ADDINS_ADDINS\)/Mono\.Addins\.dll!\$(shell pkg-config --libs mono-addins)!g' -i {} \;
find . \( -name "*.am" -or -name "*.in*" \) -exec perl -pe 's!-r\:\$\(DIR_ADDINS_GUI\)/Mono\.Addins\.Gui\.dll!\$(shell pkg-config --libs mono-addins-gui)!g' -i {} \;
find . \( -name "*.am" -or -name "*.in*" \) -exec perl -pe 's!-r\:\$\(DIR_ADDINS_SETUP\)/Mono\.Addins\.Setup\.dll!\$(shell pkg-config --libs mono-addins-setup)!g' -i {} \;

echo "Updating FlickrNet references"
find . \( -name "*.am" -or -name "*.in*" \) -exec perl -pe 's!-r\:\$\(DIR_FLICKR\)/FlickrNet\.dll!/usr/lib/cli/flickrnet-2.1.5/FlickrNet.dll!g' -i {} \;

echo "Deleting reject files (*.rej)"
find -name "*.rej" -delete
