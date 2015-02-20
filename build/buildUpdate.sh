#!/bin/bash
# server=build.palaso.org
# build_type=bt431
# root_dir=..
# $Id: d32984f53cd52f171a9cba46cd3879538ad23431 $

cd "$(dirname "$0")"

# *** Functions ***
force=0
clean=0

while getopts fc opt; do
case $opt in
f) force=1 ;;
c) clean=1 ;;
esac
done

shift $((OPTIND - 1))

copy_auto() {
if [ "$clean" == "1" ]
then
echo cleaning $2
rm -f ""$2""
else
where_curl=$(type -P curl)
where_wget=$(type -P wget)
if [ "$where_curl" != "" ]
then
copy_curl $1 $2
elif [ "$where_wget" != "" ]
then
copy_wget $1 $2
else
echo "Missing curl or wget"
exit 1
fi
fi
}

copy_curl() {
echo "curl: $2 <= $1"
if [ -e "$2" ] && [ "$force" != "1" ]
then
curl -# -L -z $2 -o $2 $1
else
curl -# -L -o $2 $1
fi
}

copy_wget() {
echo "wget: $2 <= $1"
f=$(basename $2)
d=$(dirname $2)
cd $d
wget -q -L -N $1
cd -
}


# *** Results ***
# build: Protoscript Generator-win-default Continuous (bt431)
# project: Protoscript Generator
# URL: http://build.palaso.org/viewType.html?buildTypeId=bt431
# VCS: https://github.com/sillsdev/ProtoScriptGenerator.git [master]
# dependencies:
# [0] build: geckofx29-win32-continuous (bt399)
#     project: GeckoFx
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt399
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*"=>"lib/dotnet"}
#     VCS: https://bitbucket.org/geckofx/geckofx-29.0 [default]
# [1] build: XulRunner29-win32 (bt400)
#     project: GeckoFx
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt400
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"xulrunner-29.0.1.en-US.win32.zip!**"=>"lib"}
# [2] build: palaso-win32-SILWritingSystems Continuous (bt440)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt440
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Palaso.BuildTasks.dll"=>"build/", "icu.net.dll"=>"lib/dotnet", "icudt40.dll"=>"lib/dotnet", "icuin40.dll"=>"lib/dotnet", "icuuc40.dll"=>"lib/dotnet", "L10NSharp.dll"=>"lib/dotnet", "L10NSharp.pdb"=>"lib/dotnet", "SIL.Core.dll"=>"lib/dotnet", "SIL.Core.pdb"=>"lib/dotnet", "SIL.Windows.Forms.dll"=>"lib/dotnet", "SIL.Windows.Forms.pdb"=>"lib/dotnet", "SIL.Windows.Forms.GeckoBrowserAdapter.dll"=>"lib/dotnet", "SIL.Windows.Forms.GeckoBrowserAdapter.pdb"=>"lib/dotnet", "Palaso.TestUtilities.dll"=>"lib/dotnet", "SIL.ScriptureUtils.dll"=>"lib/dotnet", "SIL.ScriptureUtils.pdb"=>"lib/dotnet", "SIL.ScriptureControls.dll"=>"lib/dotnet", "SIL.ScriptureControls.pdb"=>"lib/dotnet", "SIL.Windows.Forms.WritingSystems.dll"=>"lib/dotnet", "SIL.Windows.Forms.WritingSystems.pdb"=>"lib/dotnet", "SIL.WritingSystems.dll"=>"lib/dotnet", "SIL.WritingSystems.pdb"=>"lib/dotnet"}
#     VCS: https://github.com/sillsdev/libpalaso.git [SILWritingSystems]

# make sure output directories exist
mkdir -p ../Downloads
mkdir -p ../build/
mkdir -p ../lib
mkdir -p ../lib/dotnet

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt399/latest.lastSuccessful/Geckofx-Core.dll ../lib/dotnet/Geckofx-Core.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt399/latest.lastSuccessful/Geckofx-Core.dll.config ../lib/dotnet/Geckofx-Core.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt399/latest.lastSuccessful/Geckofx-Core.pdb ../lib/dotnet/Geckofx-Core.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt399/latest.lastSuccessful/Geckofx-Winforms.dll ../lib/dotnet/Geckofx-Winforms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt399/latest.lastSuccessful/Geckofx-Winforms.pdb ../lib/dotnet/Geckofx-Winforms.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt400/latest.lastSuccessful/xulrunner-29.0.1.en-US.win32.zip ../Downloads/xulrunner-29.0.1.en-US.win32.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/Palaso.BuildTasks.dll ../build/Palaso.BuildTasks.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/icu.net.dll ../lib/dotnet/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/icudt40.dll ../lib/dotnet/icudt40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/icuin40.dll ../lib/dotnet/icuin40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/icuuc40.dll ../lib/dotnet/icuuc40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/L10NSharp.dll ../lib/dotnet/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/L10NSharp.pdb ../lib/dotnet/L10NSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.Core.dll ../lib/dotnet/SIL.Core.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.Core.pdb ../lib/dotnet/SIL.Core.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.Windows.Forms.dll ../lib/dotnet/SIL.Windows.Forms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.Windows.Forms.pdb ../lib/dotnet/SIL.Windows.Forms.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.Windows.Forms.GeckoBrowserAdapter.dll ../lib/dotnet/SIL.Windows.Forms.GeckoBrowserAdapter.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.Windows.Forms.GeckoBrowserAdapter.pdb ../lib/dotnet/SIL.Windows.Forms.GeckoBrowserAdapter.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/Palaso.TestUtilities.dll ../lib/dotnet/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.ScriptureUtils.dll ../lib/dotnet/SIL.ScriptureUtils.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.ScriptureUtils.pdb ../lib/dotnet/SIL.ScriptureUtils.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.ScriptureControls.dll ../lib/dotnet/SIL.ScriptureControls.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.ScriptureControls.pdb ../lib/dotnet/SIL.ScriptureControls.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.Windows.Forms.WritingSystems.dll ../lib/dotnet/SIL.Windows.Forms.WritingSystems.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.Windows.Forms.WritingSystems.pdb ../lib/dotnet/SIL.Windows.Forms.WritingSystems.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.WritingSystems.dll ../lib/dotnet/SIL.WritingSystems.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt440/latest.lastSuccessful/SIL.WritingSystems.pdb ../lib/dotnet/SIL.WritingSystems.pdb
# extract downloaded zip files
unzip -uqo ../Downloads/xulrunner-29.0.1.en-US.win32.zip -d ../lib
# End of script
