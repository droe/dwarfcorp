MONO?=		mono
MCS?=		mcs
export MCS

MONOFLAGS+=	--debug

BUILDDIR:=	build.mono

DIST_DEPS:=	FNA.dll \
		FNA.dll.config \
		SharpRaven.dll \
		SharpRaven.xml \
		ICSharpCode.SharpZipLib.dll \
		Antlr4.Runtime.Standard.dll \
		Content
STEAM_DEPS:=	Steamworks.NET.dll \
		steam_appid.txt
DIST_DEPS:=	$(addprefix $(BUILDDIR)/,$(DIST_DEPS))
STEAM_DEPS:=	$(addprefix $(BUILDDIR)/,$(STEAM_DEPS))

UNAME_S:=	$(shell uname -s)
UNAME_M:=	$(shell uname -m)

FNALIBSDIR:=	DwarfCorp/DwarfCorpFNA/FNA_libs
ifeq ($(UNAME_S),Darwin)
DISTDIR?=	"$(HOME)/Library/Application Support/Steam/steamapps/common/DwarfCorp/DwarfCorp.app/Contents/MacOS"
FNALIBS:=	$(FNALIBSDIR)/osx/mono* \
		$(FNALIBSDIR)/osx/osx/*
else
ifeq ($(UNAME_S),Linux)
ifeq ($(UNAME_M),x86_64)
DISTDIR?=	"$(HOME)/.steam/steam/SteamApps/common/DwarfCorp/linux64"
FNALIBS:=	$(FNALIBSDIR)/lib64/mono* \
		$(FNALIBSDIR)/lib64/lib*
else
DISTDIR?=	"$(HOME)/.steam/steam/SteamApps/common/DwarfCorp/linux32"
FNALIBS:=	$(FNALIBSDIR)/lib/mono* \
		$(FNALIBSDIR)/lib/lib*
endif
else
$(error $(UNAME_S) $(UNAME_M) not supported)
endif
endif

SUBS=		DwarfCorp/DwarfCorpXNA DwarfCorp/LibNoise YarnSpinner


all: buildenv $(SUBS)

$(BUILDDIR):
	mkdir -p $(BUILDDIR)
	ln -s $(DISTDIR) $(BUILDDIR)/_dist_

$(DIST_DEPS): $(BUILDDIR)/%: $(BUILDDIR)/_dist_/%
	ln -sf $(<:$(BUILDDIR)/%=%) $@

$(BUILDDIR)/_pkgs_:
	mkdir $(BUILDDIR)/_pkgs_
	wget -O $(BUILDDIR)/_pkgs_/Newtonsoft.Json.nuget \
		https://www.nuget.org/api/v2/package/Newtonsoft.Json/11.0.2
	unzip -j -d $(BUILDDIR) $(BUILDDIR)/_pkgs_/Newtonsoft.Json.nuget \
		lib/net45/Newtonsoft.Json.dll lib/net45/Newtonsoft.Json.xml

pkgs: $(BUILDDIR)/_pkgs_

$(STEAM_DEPS):
	cp SteamWorks/$(notdir $@) $@

fnalibs:
	cp -r $(FNALIBS) $(BUILDDIR)

buildenv: $(BUILDDIR) $(DIST_DEPS) pkgs $(STEAM_DEPS) fnalibs

DwarfCorp/DwarfCorpXNA: DwarfCorp/LibNoise YarnSpinner

$(SUBS):
	$(MAKE) -C $@ $(SUBTARGET)

clean: SUBTARGET=clean
clean: $(SUBS)
	rm -rf $(BUILDDIR)

launch: buildenv $(SUBS)
	cd $(BUILDDIR) && $(MONO) $(MONOFLAGS) DwarfCorpFNA.mono

.PHONY: all buildenv fnalibs clean launch $(SUBS)

