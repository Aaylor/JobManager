CC=mcs
OPTIONS=-pkg:dotnet -t:library -warnaserror+
LIBRARIES=$(wildcard lib/*.dll)

SOURCES=$(wildcard src/*.cs)
DLLNAME=JobManager.dll
DLLOUTPUT=bin/$(DLLNAME)




.PHONY: all tests clean



all: $(DLLOUTPUT)

$(DLLOUTPUT): $(SOURCES)
	@mkdir -p bin/
	$(CC) $(OPTIONS) -r\:$(LIBRARIES) -out\:$(DLLOUTPUT) $(SOURCES)

clean:
	@rm -rf $(DLLOUTPUT)

