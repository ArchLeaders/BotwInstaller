import sys
from bcml.install import install_mod, refresh_master_export
from pathlib import Path

def main():
    install_mod(Path(sys.argv[1]), merge_now=bool(sys.argv[2]=='true'))

    if bool(sys.argv[2]=='true'):
        refresh_master_export()

if __name__ == '__main__':
    main()