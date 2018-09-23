MONO?=		mono
MCS?=		mcs
export MCS

MONOFLAGS+=	--debug

BUILDDIR:=	build.mono
CACHEDIR:=	build.mono.cache

DIST_DEPS:=	Content
DEPS:=		FNA.dll \
		Newtonsoft.Json.dll \
		Antlr4.Runtime.Standard.dll \
		ICSharpCode.SharpZipLib.dll \
		SharpRaven.dll \
		Steamworks.NET.dll \
		monoconfig
DIST_DEPS:=	$(addprefix $(BUILDDIR)/,$(DIST_DEPS))
DEPS:=		$(addprefix $(BUILDDIR)/,$(DEPS))

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
	mkdir $(BUILDDIR)
	ln -s $(DISTDIR) $(BUILDDIR)/_dist_

$(CACHEDIR)/FNA.zip:
	mkdir -p $(CACHEDIR)
	wget -O $@ \
		https://github.com/FNA-XNA/FNA/releases/download/18.09/FNA-1809.zip

$(BUILDDIR)/FNA.dll: $(CACHEDIR)/FNA.zip
	mkdir -p $(BUILDDIR)/_tmp_
	unzip -d $(BUILDDIR)/_tmp_ $<
	make -C $(BUILDDIR)/_tmp_/FNA
	cp $(BUILDDIR)/_tmp_/FNA/bin/Debug/* $(BUILDDIR)
	rm -rf $(BUILDDIR)/_tmp_

$(CACHEDIR)/Newtonsoft.Json.nuget:
	mkdir -p $(CACHEDIR)
	wget -O $@ https://www.nuget.org/api/v2/package/Newtonsoft.Json/9.0.1

$(BUILDDIR)/Newtonsoft.Json.dll: $(CACHEDIR)/Newtonsoft.Json.nuget
	unzip -j -d $(BUILDDIR) $< \
		lib/net45/Newtonsoft.Json.*
	touch $@

$(CACHEDIR)/Antlr4.Runtime.Standard.nuget:
	mkdir -p $(CACHEDIR)
	wget -O $@ \
		https://www.nuget.org/api/v2/package/Antlr4.Runtime.Standard/4.7.1.1

$(BUILDDIR)/Antlr4.Runtime.Standard.dll: $(CACHEDIR)/Antlr4.Runtime.Standard.nuget
	unzip -j -d $(BUILDDIR) $< \
		lib/net35/Antlr4.Runtime.Standard.*
	touch $@

$(CACHEDIR)/SharpZipLib.nuget:
	mkdir -p $(CACHEDIR)
	wget -O $@ https://www.nuget.org/api/v2/package/SharpZipLib/0.86.0

$(BUILDDIR)/ICSharpCode.SharpZipLib.dll: $(CACHEDIR)/SharpZipLib.nuget
	unzip -j -d $(BUILDDIR) $< \
		lib/20/ICSharpCode.SharpZipLib.*
	touch $@

$(CACHEDIR)/SharpRaven.nuget:
	mkdir -p $(CACHEDIR)
	wget -O $@ https://www.nuget.org/api/v2/package/SharpRaven/2.2.0

$(BUILDDIR)/SharpRaven.dll: $(CACHEDIR)/SharpRaven.nuget
	unzip -j -d $(BUILDDIR) $< \
		lib/net45/SharpRaven.*
	touch $@

$(BUILDDIR)/Steamworks.NET.dll:
	cp SteamWorks/Steamworks.NET.dll $(BUILDDIR)
	cp SteamWorks/steam_appid.txt $(BUILDDIR)

$(BUILDDIR)/monoconfig:
	cp -r $(FNALIBS) $(BUILDDIR)

$(DIST_DEPS): $(BUILDDIR)/%: $(BUILDDIR)/_dist_/%
	ln -sf $(<:$(BUILDDIR)/%=%) $@

buildenv: $(BUILDDIR) $(DEPS) $(DIST_DEPS)

DwarfCorp/DwarfCorpXNA: DwarfCorp/LibNoise YarnSpinner

$(SUBS):
	$(MAKE) -C $@ $(SUBTARGET)

clean: SUBTARGET=clean
clean: $(SUBS)
	rm -rf $(BUILDDIR) profiler.report

realclean: clean
	rm -rf $(CACHEDIR)

launch: buildenv $(SUBS)
	cd $(BUILDDIR) && $(MONO) $(MONOFLAGS) DwarfCorpFNA.mono

# https://www.mono-project.com/docs/debug+profile/profile/profiler/
profile: MONOFLAGS+=--profile=log:alloc,calls,report,output=../profiler.report
profile: launch
	@echo Mono profiler report written to: profiler.report

.PHONY: all buildenv fnalibs clean launch $(SUBS)

