MCS?=		mcs
OBJDIR:=	../obj.mono

TARGET=		$(OBJDIR)/YarnSpinner.dll

CSFLAGS+=	-debug
#CSFLAGS+=	-define:"TRACE;DEBUG"
#CSFLAGS+=	-langversion:7

SYSREFS=	System.dll
LIBREFS=	Antlr4.Runtime.Standard.dll \
		Newtonsoft.Json.dll

LIBDIR=		$(OBJDIR)
CSFLAGS+=	$(addprefix -r:,$(SYSREFS) $(addprefix $(LIBDIR)/,$(LIBREFS)))


all: $(TARGET)

$(TARGET): $(OBJDIR)
	$(MCS) $(CSFLAGS) -recurse:'*.cs' -target:library -out:$@

clean:
	rm -f $(TARGET)

.PHONY: all clean

