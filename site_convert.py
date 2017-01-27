import argparse
import os
import subprocess

def all_html_files_in_tree(root_dir):
    print("Walking {}".format(root_dir))
    for subdir, dirs, files in os.walk(root_dir):
        print("Reading from {}".format(subdir))
        for filename in files:
            filepath = os.path.join(subdir, filename)
            if filepath.endswith(".html"):
                yield filepath

def html2rst(html):
    p = subprocess.Popen(['pandoc', '-f',  'html', '-t', 'html'], stdin=subprocess.PIPE, stdout=subprocess.PIPE)
    return p.communicate(html.encode('utf-8'))[0]

def getfilepath(filepath):
    path, file = os.path.split(filepath)
    path, lastdir = os.path.split(path)
    return '.' + '/' + lastdir

def getfilename(filepath):
    return os.path.basename(os.path.splitext(filepath)[0]) + '.rst'

def main(path):
    for filepath in all_html_files_in_tree(path):
        print(filepath)
        with open(filepath, 'r') as input:
            html = input.read()
            rst = html2rst(html).decode('utf-8')

        # ToDo: Need to make the dir, if it does not exist
        # At top level i.e. Paramore.Brighter root, we don't want to make a sub-dir i.e. grabbing last element of path fails for subdir here.
        # We need to understand layout of sphinx docs files, to understand how to build rst structure anyway.
        output_filename = getfilepath(filepath) + '/' + getfilename(filepath)
        print(output_filename)
        print(rst)
        with open(output_filename,'tw+', encoding='utf-8') as output:
            output.write(rst + '\n')

    print("All files read")

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("root")
    args = parser.parse_args()
    main(args.root)

