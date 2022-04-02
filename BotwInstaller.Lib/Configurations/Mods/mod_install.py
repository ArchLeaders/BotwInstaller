import sys
from bcml.install import install_mod
from pathlib import Path

def main():
    install_mod(Path(sys.argv[1]), merge_now=False)

if __name__ == '__main__':
    main()