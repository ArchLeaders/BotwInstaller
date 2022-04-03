import sys
from bcml.install import install_mod
from pathlib import Path

def main():
    install_mod(Path(sys.argv[1]), merge_now=bool(sys.argv[2] == "true"), insert_priority=int(sys.argv[3]))

if __name__ == '__main__':
    main()