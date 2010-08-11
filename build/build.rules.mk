UNIQUE_FILTER_PIPE = tr [:space:] \\n | sort | uniq
BUILD_DATA_DIR = $(top_builddir)/bin/share/$(PACKAGE)

# Since all other attempts failed, we currently go this way:
# This code adds the file specified in ASSEMBLY_INFO_SOURCE to SOURCES_BUILD.
# If no such file is specified, the default AssemblyInfo.cs is used.
ASSEMBLY_INFO_SOURCE_REAL = \
	$(shell if [ "$(ASSEMBLY_INFO_SOURCE)" ]; \
	then \
		echo "$(addprefix $(srcdir)/, $(ASSEMBLY_INFO_SOURCE))"; \
	else \
		echo "$(top_srcdir)/src/AssemblyInfo.cs"; \
	fi)

SOURCES_BUILD = $(addprefix $(srcdir)/, $(SOURCES))
SOURCES_BUILD += $(ASSEMBLY_INFO_SOURCE_REAL)


RESOURCES_EXPANDED = $(addprefix $(srcdir)/, $(RESOURCES))
RESOURCES_BUILD = $(foreach resource, $(RESOURCES_EXPANDED), \
	-resource:$(resource),$(notdir $(resource)))

INSTALL_ICONS = $(top_srcdir)/build/private-icon-theme-installer "$(mkinstalldirs)" "$(INSTALL_DATA)"
THEME_ICONS_SOURCE = $(wildcard $(srcdir)/ThemeIcons/*/*/*.png) $(wildcard $(srcdir)/ThemeIcons/scalable/*/*.svg)
THEME_ICONS_RELATIVE = $(subst $(srcdir)/ThemeIcons/, , $(THEME_ICONS_SOURCE))

ASSEMBLY_EXTENSION = $(strip $(patsubst library, dll, $(TARGET)))
ASSEMBLY_FILE = $(top_builddir)/bin/$(ASSEMBLY).$(ASSEMBLY_EXTENSION)

INSTALL_DIR_RESOLVED = $(firstword $(subst , $(DEFAULT_INSTALL_DIR), $(INSTALL_DIR)))

if ENABLE_TESTS
    LINK += " $(NUNIT_LIBS)"
    ENABLE_TESTS_FLAG = "-define:ENABLE_TESTS"
endif

if ENABLE_ATK
    ENABLE_ATK_FLAG = "-define:ENABLE_ATK"
endif

FILTERED_LINK = $(shell echo "$(LINK)" | $(UNIQUE_FILTER_PIPE))
DEP_LINK = $(shell echo "$(LINK)" | $(UNIQUE_FILTER_PIPE) | sed s,-r:,,g | grep '$(top_builddir)/bin/')

OUTPUT_FILES = \
	$(ASSEMBLY_FILE) \
	$(ASSEMBLY_FILE).mdb

moduledir = $(INSTALL_DIR_RESOLVED)
module_SCRIPTS = $(OUTPUT_FILES)

all-local: theme-icons

run: 
	@pushd $(top_builddir); \
	make run; \
	popd;

# uncommented for now.
# tests are currently excuted from Makefile in $(top_builddir)
#test:
#	@pushd $(top_builddir)/tests; \
#	make $(ASSEMBLY); \
#	popd;

build-debug:
	@echo $(DEP_LINK)

$(ASSEMBLY_FILE).mdb: $(ASSEMBLY_FILE)

$(ASSEMBLY_FILE): $(SOURCES_BUILD) $(RESOURCES_EXPANDED) $(DEP_LINK)
	@mkdir -p $(top_builddir)/bin
	@if [ ! "x$(ENABLE_RELEASE)" = "xyes" ]; then \
		$(top_srcdir)/build/dll-map-makefile-verifier $(srcdir)/Makefile.am $(srcdir)/$(notdir $@.config) && \
		$(MONO) $(top_builddir)/build/dll-map-verifier.exe $(srcdir)/$(notdir $@.config) -iwinmm -ilibbanshee -ilibbnpx11 -ilibc -ilibc.so.6 -iintl -ilibmtp.dll -ilibigemacintegration.dylib -iCFRelease $(SOURCES_BUILD); \
	fi;
	$(MCS) \
		$(GMCS_FLAGS) \
		$(ASSEMBLY_BUILD_FLAGS) \
		-nowarn:0278 -nowarn:0078 $$warn \
		-define:HAVE_GTK_2_10 -define:NET_2_0 \
		-debug -target:$(TARGET) -out:$@ \
		$(BUILD_DEFINES) $(CSC_DEFINES) $(ENABLE_TESTS_FLAG) $(ENABLE_ATK_FLAG) \
		$(FILTERED_LINK) $(RESOURCES_BUILD) $(SOURCES_BUILD)
	@if [ -e $(srcdir)/$(notdir $@.config) ]; then \
		cp $(srcdir)/$(notdir $@.config) $(top_builddir)/bin; \
	fi;
	@if [ ! -z "$(EXTRA_BUNDLE)" ]; then \
		cp $(EXTRA_BUNDLE) $(top_builddir)/bin; \
	fi;

theme-icons: $(THEME_ICONS_SOURCE)
	@$(INSTALL_ICONS) -il "$(BUILD_DATA_DIR)" "$(srcdir)" $(THEME_ICONS_RELATIVE)

install-data-local: $(THEME_ICONS_SOURCE)
	@$(INSTALL_ICONS) -i "$(DESTDIR)$(pkgdatadir)" "$(srcdir)" $(THEME_ICONS_RELATIVE)
	
uninstall-local: $(THEME_ICONS_SOURCE)
	@$(INSTALL_ICONS) -u "$(DESTDIR)$(pkgdatadir)" "$(srcdir)" $(THEME_ICONS_RELATIVE)

EXTRA_DIST = $(SOURCES_BUILD) $(RESOURCES_EXPANDED) $(THEME_ICONS_SOURCE)

CLEANFILES = $(OUTPUT_FILES) $(ASSEMBLY_FILE).config
DISTCLEANFILES = *.pidb
MAINTAINERCLEANFILES = Makefile.in

