import sys
from bcml.install import install_mod
from pathlib import Path

def main():
    install_mod(Path(sys.argv[1]), merge_now=False, insert_priority=int(sys.argv[2]))

if __name__ == '__main__':
    main()