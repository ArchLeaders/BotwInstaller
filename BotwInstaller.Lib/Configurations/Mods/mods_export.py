from bcml import mergers
from bcml.install import link_master_mod

def main():

    for merger in [m() for m in mergers.get_mergers()]:
        merger.perform_merge()

    link_master_mod()

if __name__ == '__main__':
    main()