import os, gzip

INPUT_FILES=['kr_codepoints.json','ja_codepoints.json','zh_codepoints.json','vn_codepoints.json','yue_codepoints.json','unicodepoints.json']

for f in INPUT_FILES:
        source = os.path.join('result', f)
        input = open(source, 'rb')
        s = input.read()
        input.close()
        dest = os.path.join('../src/Unihandecode/_gz', f+'.gz')
        output = gzip.GzipFile(dest, 'wb')
        output.write(s)
        output.close()
print("done")