MCS?=		mcs
BUILDDIR:=	../build.mono

TARGET=		$(BUILDDIR)/YarnSpinner.dll

CSFLAGS+=	-debug
#CSFLAGS+=	-define:"TRACE;DEBUG"
#CSFLAGS+=	-langversion:7

SYSREFS=	System.dll
LIBREFS=	Antlr4.Runtime.Standard.dll \
		Newtonsoft.Json.dll

LIBDIR=		$(BUILDDIR)
CSFLAGS+=	$(addprefix -r:,$(SYSREFS) $(addprefix $(LIBDIR)/,$(LIBREFS)))


all: $(TARGET)

$(TARGET): $(shell find . -name '*.cs') GNUmakefile
	$(MCS) $(CSFLAGS) -recurse:'*.cs' -target:library -out:$@

clean:
	rm -f $(TARGET)

.PHONY: all clean

