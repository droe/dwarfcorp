MONO?=		mono
MCS?=		mcs
export MCS

MONOFLAGS+=	--debug

OBJDIR:=	obj.mono

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
APP_DEPS:=	$(addprefix $(OBJDIR)/,$(APP_DEPS))
STEAM_DEPS:=	$(addprefix $(OBJDIR)/,$(STEAM_DEPS))

UNAME_S:=	$(shell uname -s)
UNAME_M:=	$(shell uname -m)

FNALIBSDIR:=	DwarfCorp/DwarfCorpFNA/FNA_libs
ifeq ($(UNAME_S),Darwin)
APPDIR?=	"$(HOME)/Library/Application Support/Steam/steamapps/common/DwarfCorp/DwarfCorp.app/Contents/MacOS"
FNALIBS:=	$(FNALIBSDIR)/osx/mono* \
		$(FNALIBSDIR)/osx/osx/*
else
ifeq ($(UNAME_S),Linux)
ifeq ($(UNAME_M),x86_64)
APPDIR?=	"$(HOME)/.steam/steam/SteamApps/common/DwarfCorp/linux64"
FNALIBS:=	$(FNALIBSDIR)/lib64/mono* \
		$(FNALIBSDIR)/lib64/lib*
else
APPDIR?=	"$(HOME)/.steam/steam/SteamApps/common/DwarfCorp/linux32"
FNALIBS:=	$(FNALIBSDIR)/lib/mono* \
		$(FNALIBSDIR)/lib/lib*
endif
else
$(error $(UNAME_S) $(UNAME_M) not supported)
endif
endif

SUBS=		DwarfCorp/DwarfCorpXNA DwarfCorp/LibNoise YarnSpinner


all: objdir $(SUBS)

$(OBJDIR):
	mkdir -p $(OBJDIR)
	ln -s $(APPDIR) $(OBJDIR)/_app_

$(APP_DEPS): $(OBJDIR)/%: $(OBJDIR)/_app_/%
	ln -sf $(<:$(OBJDIR)/%=%) $@

$(STEAM_DEPS):
	cp SteamWorks/$(notdir $@) $@

fnalibs:
	cp -r $(FNALIBS) $(OBJDIR)

objdir: $(OBJDIR) $(APP_DEPS) $(STEAM_DEPS) fnalibs

DwarfCorp/DwarfCorpXNA: DwarfCorp/LibNoise YarnSpinner

$(SUBS):
	$(MAKE) -C $@ $(SUBTARGET)

clean: SUBTARGET=clean
clean: $(SUBS)
	rm -rf $(OBJDIR)

launch: objdir $(SUBS)
	cd $(OBJDIR) && $(MONO) $(MONOFLAGS) DwarfCorpFNA.mono.osx

.PHONY: all objdir fnalibs clean launch $(SUBS)

