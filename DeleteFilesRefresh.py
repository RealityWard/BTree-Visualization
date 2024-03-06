import os
import argparse
import re
from pathlib import Path

_PathsToWorkOn = []
_FilesToWorkOn = []

parser = argparse.ArgumentParser(description="Delete folders and files with certain extensions")
parser.add_argument('--folders', help="Names of folders to delete in cwd seperated by ','", default='bin,obj', nargs='?', )
parser.add_argument('--fileEx', help="File Extensions seperated by ','", default='.sln', nargs='?')
args = parser.parse_args()

print("These are the found paths:")
for root, dirs, files in os.walk('.'):
  for dir in dirs:
    for target in args.folders.split(','):
      if dir == target:
        _PathsToWorkOn.append(os.path.join(root, dir))
        print('dir: ' + os.path.join(root, dir))
  for file in files:
    for target in args.folders.split(','):
      if re.search(r'\W' + target + r'\b', root) != None:
        _FilesToWorkOn.append(os.path.join(root, file))
        print('file: ' + os.path.join(root, file))
    for ext in args.fileEx.split(','):
      if file.endswith(ext):
        _FilesToWorkOn.append(os.path.join(root, file))
        print('file: ' + os.path.join(root, file))

if input("Are you sure you wish to delete the found files?(y/n)") == 'y':
  for file in _FilesToWorkOn:
    try:
      Path.unlink(Path(file),missing_ok=True)
      print("Deleted: " + file)
    except:
      print("Couldn't delete: " + file)
  for path in _PathsToWorkOn:
    try:
      Path.rmdir(Path(path))
      print("Deleted: " + path)
    except:
      print("Couldn't delete: " + path)