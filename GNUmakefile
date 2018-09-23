MONO?=		mono
MCS?=		mcs
export MCS

MONOFLAGS+=	--debug

OBJDIR:=	obj.mono

# use app bundle installed by Steam by default
APPBUNDLE?=	"$(HOME)/Library/Application Support/Steam/steamapps/common/DwarfCorp/DwarfCorp.app"
APPDEPS:=	FNA.dll \
		FNA.dll.config \
		Newtonsoft.Json.dll \
		Newtonsoft.Json.xml \
		SharpRaven.dll \
		SharpRaven.xml \
		YarnSpinner.dll \
		Steamworks.NET.dll \
		ICSharpCode.SharpZipLib.dll \
		Antlr4.Runtime.Standard.dll \
		monoconfig \
		monomachineconfig \
		steam_appid.txt \
		osx \
		Content
APPDEPS:=	$(addprefix $(OBJDIR)/,$(APPDEPS))

SUBS=		DwarfCorp/DwarfCorpXNA DwarfCorp/LibNoise

all: objdir $(SUBS)

$(OBJDIR):
	mkdir -p $(OBJDIR)
	ln -s $(APPBUNDLE) $(OBJDIR)/_app_

$(APPDEPS): $(OBJDIR)/%: $(OBJDIR)/_app_/Contents/MacOS/%
	ln -sf $(<:$(OBJDIR)/%=%) $@

objdir: $(OBJDIR) $(APPDEPS)

DwarfCorp/DwarfCorpXNA: DwarfCorp/LibNoise

$(SUBS):
	$(MAKE) -C $@ $(SUBTARGET)

clean: SUBTARGET=clean
clean: $(SUBS)
	rm -rf $(OBJDIR)

launch: objdir $(SUBS)
	cd $(OBJDIR) && DYLD_LIBRARY_PATH=./osx $(MONO) $(MONOFLAGS) DwarfCorpFNA.mono.osx

.PHONY: all objdir clean launch $(SUBS)

