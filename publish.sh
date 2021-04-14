
VERSION=(git tag --sort=-version:refname | head -1 | sed 's/v\(.*\)/\1/')
PACKAGEDIR=$PWD/Packages
dotnet build --configuration Release
rm -rf $PACKAGEDIR
dotnet pack --no-build --configuration Release -o $PACKAGEDIR /p:PackageVersion=$VERSION -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
for f in $PACKAGEDIR/*.nupkg; do
dotnet nuget push --skip-duplicate $f --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
done
