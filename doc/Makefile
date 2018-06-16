.PHONY: lualatex

lualatex: PZ.tex
	rm -f *.toc *.aux
	lualatex $<
	lualatex $<

#clean:
#	latexmk -C PZ.tex
