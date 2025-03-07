APP_NAME="PixelViewer"
FRAMEWORK="net8.0"
RID_LIST=("linux-x64" "linux-arm64")
CONFIG="Release"
TRIM_ASSEMBLIES="true"
READY_TO_RUN="false"

echo "********** Start building $APP_NAME **********"

# Get application version
VERSION=$(dotnet run --project PackagingTool get-current-version $APP_NAME/$APP_NAME.csproj)
if [ "$?" != "0" ]; then
    echo "Unable to get version of $APP_NAME"
    exit
fi
echo "Version: $VERSION"
PREV_VERSION=$(dotnet run --project PackagingTool get-previous-version $APP_NAME/$APP_NAME.csproj $VERSION)
if [ ! -z "$PREV_VERSION" ]; then
    echo "Previous version: $PREV_VERSION"
fi

# Create output directory
if [[ ! -d "./Packages/$VERSION" ]]; then
    echo "Create directory 'Packages/$VERSION'"
    mkdir -p ./Packages/$VERSION
    if [ "$?" != "0" ]; then
        exit
    fi
fi

# Build packages
for i in "${!RID_LIST[@]}"; do
    RID=${RID_LIST[$i]}

    echo " " 
    echo "[$RID]"
    echo " "

    # clean
    rm -r ./$APP_NAME/bin/$CONFIG/$FRAMEWORK/$RID
    dotnet restore $APP_NAME -r $RID
    if [ "$?" != "0" ]; then
        exit
    fi
    dotnet clean $APP_NAME -c $CONFIG -r $RID
    if [ "$?" != "0" ]; then
        exit
    fi
    
    # build
    dotnet publish $APP_NAME -c $CONFIG -r $RID --self-contained true -p:PublishTrimmed=$TRIM_ASSEMBLIES -p:PublishReadyToRun=$READY_TO_RUN
    if [ "$?" != "0" ]; then
        exit
    fi

    # zip package
    # ditto -c -k --sequesterRsrc "./$APP_NAME/bin/$CONFIG/$FRAMEWORK/$RID/publish/" "./Packages/$VERSION/$APP_NAME-$VERSION-$RID.zip"
    zip -r "./Packages/$VERSION/$APP_NAME-$VERSION-$RID.zip" "./$APP_NAME/bin/$CONFIG/$FRAMEWORK/$RID/publish/"
    if [ "$?" != "0" ]; then
        exit
    fi

done

# Generate diff packages
if [ ! -z "$PREV_VERSION" ]; then
    dotnet run --project PackagingTool create-diff-packages linux $PREV_VERSION $VERSION
fi

# Generate package manifest
# dotnet run --project PackagingTool create-package-manifest linux $APP_NAME $VERSION