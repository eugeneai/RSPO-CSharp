.PHONY: lualatex

FLAGS=-file-line-error -shell-escape --synctex=1

lualatex: PZ.tex
	rm -f *.toc *.aux
	lualatex $(FLAGS) $<
	lualatex $(FLAGS) $<

#clean:
#	latexmk -C PZ.tex
