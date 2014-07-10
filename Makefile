CC=mcs
OPTIONS=-pkg:dotnet -t:library #-warnaserror+
LIBRARIES=$(wildcard lib/*.dll)

SOURCES=$(wildcard src/*.cs)
DLLNAME=JobManager.dll
DLLOUTPUT=bin/$(DLLNAME)

TESTS=$(notdir $(wildcard tests/*.cs))
TESTSOPT=




.PHONY: all tests clean



all: $(DLLOUTPUT)

$(DLLOUTPUT): $(SOURCES)
	@mkdir -p bin/
	$(CC) $(OPTIONS) -r\:$(LIBRARIES) -out\:$(DLLOUTPUT) $(SOURCES)

tests: $(DLLOUTPUT)
	@echo "\n\n -- BUILDING TESTS --\n"
	@cp $(DLLOUTPUT) tests
	@$(foreach TESTFILE, $(TESTS), \
		LP=`pwd` && cd tests && \
		$(CC) -r:$(DLLNAME) $(TESTSOPT) $(TESTFILE) && \
		LD_LIBRARY_PATH=\"tests\" mono $(TESTFILE:.cs=.exe) \
		cp $(LP))
	@rm tests/$(DLLNAME)

clean:
	@rm -rf $(DLLOUTPUT)
	@rm -rf $(wildcard tests/*.exe)

