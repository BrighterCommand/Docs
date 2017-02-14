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
    p = subprocess.Popen(['pandoc', '-f',  'html', '-t', 'rst'], stdin=subprocess.PIPE, stdout=subprocess.PIPE)
    return p.communicate(html.encode('utf-8'))[0]

def getdir(filepath):
    path, file = os.path.split(filepath)
    path, lastdir = os.path.split(path)
    return '.' + '/' + lastdir

def getfilename(filepath):
    filename = os.path.split(filepath)[1]
    filename_no_ext = os.path.splitext(filename)[0]
    return filename_no_ext + '.rst'

def main(path):
    for filepath in all_html_files_in_tree(path):
        print(filepath)
        with open(filepath, 'r') as input:
            html = input.read()
            rst = html2rst(html).decode('utf-8')

        directory = getdir(filepath)
        lowercase_dir = directory.lower()
        output_filename = lowercase_dir  + '/' + getfilename(filepath)
        print(output_filename)

        if not os.path.exists(lowercase_dir):
            os.makedirs(lowercase_dir)

        print("Writing: {}".format(output_filename))
        with open(output_filename,'tw+', encoding='utf-8') as output:
            output.write(rst + '\n')

    print("All files read")

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("root")
    args = parser.parse_args()
    main(args.root)

