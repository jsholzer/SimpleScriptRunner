#!python3
# Refernces: 
# https://docs.microsoft.com/en-us/nuget/quickstart/create-and-publish-a-package-using-the-dotnet-cli
# https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet?tabs=netcore21
#
# To release a new version
# 1) From IDE or edit project file, change SimpleScriptRunnerBto.csproj by increasing the "PackageVersion" number
# 2) Run this script with a target of nuget
# 3) Find output (like .out\SimpleScriptRunnerBto.1.0.11.nupkg) and publish to nuget
#
import subprocess, argparse, os, zipfile, shutil, glob

parser = argparse.ArgumentParser(description='Builds SimpleScriptRunnerBto')
parser.add_argument('target', help='Target of either: nuget')

args = parser.parse_args()

def resetDirectory(path):
    if os.path.exists(path):
        shutil.rmtree(path)
    else:
        os.mkdir(path)
    print("Verified Dependency: resetDirectory=" + path)

def verifyDependencyDotNet():
    subprocess.run(['dotnet','--info'], check=True, capture_output=True)
    print("Verified Dependency: DotNet")    

def verifyDependencyNuget():
    subprocess.run(['nuget','help'], check=True, capture_output=True)
    print("Verified Dependency: Nuget")    

def copyDir(source, dest):
    for toCopy in glob.glob(source + "/*"):
        shutil.copy(toCopy, dest)        
    
def packageDir(path, name):
    with zipfile.ZipFile(os.path.join(path, name), 'w', zipfile.ZIP_DEFLATED) as cmdZip:
        for root, dirs, files in os.walk(path):
            for file in files:
                if file != name:
                    file = os.path.join(root, file)
                    arcname = file.replace(path,'')
                    cmdZip.write(file, arcname=arcname)
             
def buildNuget():
    verifyDependencyDotNet()    
    resetDirectory('.out')
       
    # Cleans build
    print("\n\nRunning Step: Clean")
    subprocess.run(['dotnet','clean','--configuration','Release'], check=True)

    print("\n\nRunning Step: Restore")
    subprocess.run(['dotnet','restore','SimpleScriptRunnerBto/SimpleScriptRunnerBto.csproj'], check=True)
	
    # Pack build
    print("\n\nRunning Step: Pack")
    subprocess.run(['dotnet','pack','--output','.out','SimpleScriptRunnerBto/SimpleScriptRunnerBto.csproj'], check=True)
    
    return



##########################
## Runs build
#########################  
    
# Downloads package from build server
if args.target == 'nuget':
    buildNuget()
else:
    raise Exception('Unknown target ' + args.target)
    
print("SUCCESS: build '{0}' complete".format(args.target))






