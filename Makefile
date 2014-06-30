CC=mcs
OPTIONS=-pkg:dotnet -warnaserror+ -t:library
LIBRARIES=
SOURCES=$(wildcard *.cs)
DLLOUTPUT=JobManager.dll



.PHONY: clean

all: construct_lib

construct_lib: $(SOURCES)
	@echo "SOURCES:: $(SOURCES)"
	$(CC) $(OPTIONS) -out\:$(DLLOUTPUT) $(SOURCES)

clean:
	rm -rf $(DLLOUTPUT)

