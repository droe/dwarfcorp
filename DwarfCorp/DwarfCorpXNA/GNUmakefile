MCS?=		mcs
OBJDIR:=	../../obj.mono

TARGET=		$(OBJDIR)/DwarfCorpFNA.mono.osx

CSFLAGS+=	-debug
CSFLAGS+=	-define:"GEMFNA;WINDOWS"
#CSFLAGS+=	-langversion:7

SYSREFS=	System.dll \
		System.Data.dll \
		System.Data.Linq.dll \
		System.Management.dll
LIBREFS=	FNA.dll \
		LibNoise.dll \
		Newtonsoft.Json.dll \
		SharpRaven.dll \
		YarnSpinner.dll \
		Steamworks.NET.dll \
		ICSharpCode.SharpZipLib.dll \
		Antlr4.Runtime.Standard.dll

LIBDIR=		$(OBJDIR)
CSFLAGS+=	$(addprefix -r:,$(SYSREFS) $(addprefix $(LIBDIR)/,$(LIBREFS)))


all: $(TARGET)

$(TARGET):
	$(MCS) $(CSFLAGS) -recurse:'*.cs' -target:exe -out:$@

clean:
	rm -f $(TARGET)

.PHONY: all install launch clean

