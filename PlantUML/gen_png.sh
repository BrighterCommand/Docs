#!/bin/sh
# converts all puml files to png

#act
for FILE in *.puml; do
        echo Converting $FILE..
        FILE_PNG=${FILE//puml/png}
        # cat $FILE | docker run --rm -i think/plantuml > $FILE_PNG
       java -DPLANTUML_LIMIT_SIZE=12284 -jar plantuml.jar $FILE -O . 
done
mv *.png ../source/_static/images/
echo Done