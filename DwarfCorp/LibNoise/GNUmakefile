MCS?=		mcs
OBJDIR:=	../../obj.mono

TARGET=		$(OBJDIR)/LibNoise.dll

CSFLAGS+=	-debug
#CSFLAGS+=	-define:"TRACE;DEBUG"
#CSFLAGS+=	-langversion:7

SYSREFS=	System.dll \
		System.Data.dll \
		System.Xml.dll
LIBREFS=	

LIBDIR=		$(OBJDIR)
CSFLAGS+=	$(addprefix -r:,$(SYSREFS) $(addprefix $(LIBDIR)/,$(LIBREFS)))


all: $(TARGET)

$(OBJDIR):
	mkdir -p $(OBJDIR)

$(TARGET): $(OBJDIR)
	$(MCS) $(CSFLAGS) -recurse:'*.cs' -target:library -out:$@

clean:
	rm -f $(TARGET)

.PHONY: all clean

