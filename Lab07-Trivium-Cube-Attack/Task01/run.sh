#!/bin/bash

PROJECT_DIR="./Task01.csproj"
PUBLISH_DIR="./publish"
RUNTIME="linux-x64" 
CONFIG="Release"
APP_NAME="Task01"

sudo sysctl -w vm.nr_hugepages=512

echo "[AOT BUILD] Cleaning workspace..."
rm -rf $PUBLISH_DIR
dotnet clean -c $CONFIG > /dev/null

echo "[AOT BUILD] Compiling NATIVE binary for Zen 3..."

dotnet publish $PROJECT_DIR \
    -c $CONFIG \
    -r $RUNTIME \
    --self-contained true \
    -p:PublishAot=true \
    -p:IlcOptimizationPreference=Speed \
    -p:IlcInstructionSet=x86-64-v3 \
    -o $PUBLISH_DIR

if [ $? -ne 0 ]; then
    echo "❌ Native Compilation failed! See errors above!"
    exit 1
fi

echo "[RUN] Configuring Zen 3 Environment..."

export DOTNET_gcServer=1
export DOTNET_LargePageMemory=1

echo "⚡ [EXEC] Launching Native $APP_NAME with Real-Time Priority..."
chmod +x $PUBLISH_DIR/$APP_NAME

sudo nice -n -20 $PUBLISH_DIR/$APP_NAME