import sys
from bcml import install
from pathlib import Path

def main():
    install.install_mod(Path(sys.argv[1]), merge_now=bool(sys.argv[2]=='true'))

input()

if __name__ == '__main__':
    main()