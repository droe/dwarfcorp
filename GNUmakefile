MONO?=		mono
MCS?=		mcs
export MCS

MONOFLAGS+=	--debug

OBJDIR:=	obj.mono

# use app bundle installed by Steam by default
APPBUNDLE?=	"$(HOME)/Library/Application Support/Steam/steamapps/common/DwarfCorp/DwarfCorp.app"

APP_DEPS:=	FNA.dll \
		FNA.dll.config \
		Newtonsoft.Json.dll \
		Newtonsoft.Json.xml \
		SharpRaven.dll \
		SharpRaven.xml \
		ICSharpCode.SharpZipLib.dll \
		Antlr4.Runtime.Standard.dll \
		Content
STEAM_DEPS:=	Steamworks.NET.dll \
		steam_appid.txt
OSX_DEPS:=	monoconfig \
		monomachineconfig \
		osx
APP_DEPS:=	$(addprefix $(OBJDIR)/,$(APP_DEPS))
STEAM_DEPS:=	$(addprefix $(OBJDIR)/,$(STEAM_DEPS))
OSX_DEPS:=	$(addprefix $(OBJDIR)/,$(OSX_DEPS))
ALL_DEPS:=	$(APP_DEPS) $(STEAM_DEPS) $(OSX_DEPS)

SUBS=		DwarfCorp/DwarfCorpXNA DwarfCorp/LibNoise YarnSpinner


all: objdir $(SUBS)

$(OBJDIR):
	mkdir -p $(OBJDIR)
	ln -s $(APPBUNDLE) $(OBJDIR)/_app_

$(APP_DEPS): $(OBJDIR)/%: $(OBJDIR)/_app_/Contents/MacOS/%
	ln -sf $(<:$(OBJDIR)/%=%) $@

$(STEAM_DEPS):
	rm -f $@
	cp SteamWorks/$(notdir $@) $@

$(OSX_DEPS):
	rm -rf $@
	cp -r DwarfCorp/DwarfCorpFNA/FNA_libs/osx/$(notdir $@) $@

objdir: $(OBJDIR) $(ALL_DEPS)

DwarfCorp/DwarfCorpXNA: DwarfCorp/LibNoise YarnSpinner

$(SUBS):
	$(MAKE) -C $@ $(SUBTARGET)

clean: SUBTARGET=clean
clean: $(SUBS)
	rm -rf $(OBJDIR)

launch: objdir $(SUBS)
	cd $(OBJDIR) && DYLD_LIBRARY_PATH=./osx $(MONO) $(MONOFLAGS) DwarfCorpFNA.mono.osx

.PHONY: all objdir clean launch $(SUBS)

