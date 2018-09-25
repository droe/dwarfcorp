MCS?=		mcs
BUILDDIR:=	../../build.mono

TARGET=		$(BUILDDIR)/LibNoise.dll

CSFLAGS+=	-debug
#CSFLAGS+=	-define:"TRACE;DEBUG"
#CSFLAGS+=	-langversion:7

SYSREFS=	System.dll \
		System.Data.dll \
		System.Xml.dll
LIBREFS=	

LIBDIR=		$(BUILDDIR)
CSFLAGS+=	$(addprefix -r:,$(SYSREFS) $(addprefix $(LIBDIR)/,$(LIBREFS)))


all: $(TARGET)

$(TARGET): $(shell find . -name '*.cs') GNUmakefile
	$(MCS) $(CSFLAGS) -recurse:'*.cs' -target:library -out:$@

clean:
	rm -f $(TARGET)

.PHONY: all clean

