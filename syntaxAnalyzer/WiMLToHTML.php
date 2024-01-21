<?php
function split($string, $separator){
    return preg_split("/" . $separator . "/", $string, -1, PREG_SPLIT_NO_EMPTY);
}
function preg_match_of_normal_human($regex, $string){
    preg_match($regex, $string, $matches);
    return $matches;
}
class Element {
    public $Parent;
    public $CoreElement;
    public $Children = array();
    public $Name;
    public $Param;
    public $Content;
    public $isProcessed  = false;

    public function __construct($parent, $coreElement, $name, $param, $content) {
        $this->Parent = $parent;
        $this->CoreElement = $coreElement;
        $this->Name = $name;
        $this->Param = $param;
        $this->Content = $content;
    }

    public function __toString() {
        $t = $this->Parent;
        $i = 0;
        while ($t != null) {
            $i++;
            $t = $t->Parent;
        }
        $ts = "";
        for ($j = 0; $j < $i; $j++) {
            $ts .= "  ";
        }
        $tt = "";
        if (!is_null($this->Param)) {
            $tt = implode(" ", $this->Param);
        }
        if ($this->Name == "text") {
            $tt .= "\n" . $ts . "  " . $this->Content;
        }
        return $ts . $this->Name . " " . $tt;
    }
}
class WiMLToHTML
{
    public static function Convert($WiML)
    {
        $html = "";
        $core = new CoreElement($WiML);
        $core->Process();
        //$core->PrintAST();
        $html .= "<h1>{$core->title}</h1>";
        if ($core->Card !== null) {
            $html .= "<div class=\"card\"><h3>$core->title</h3>" . self::childrenToHtml($core->Card) . "</div>";
        }
        $html .= "<div class=\"main\">" . self::childrenToHtml($core->Body) . "</div>";
        if ($core->Source !== null) {
            $html .= "<div class=\"source\">" . self::childrenToHtml($core->Source) . "</div>";
        }
        return $html;
    }

    private static function childrenToHtml($el)
    {
        return implode("", array_map(function ($x) {
            return self::ElementToHTML($x);
        }, $el->Children));
    }

    private static function ElementToHTML($element)
    {
        $html = "";
        switch ($element->Name) {
            case "img":
                $html .= "<img src=" .  split($element->Param[0],"=")[1];
                if (count($element->Param) > 1) {
                    $html .= "alt=" . split($element->Param[1], "=")[1] . "/>";
                    $html .= '<div class=\"img-description\">' . trim(split($element->Param[1], "=")[1], '"') . '</div>';
                }
                else{
                    $html .= "/>";
                    $html .= " " . var_dump($element->Param);
                }
                break;
            case "d":
                $html .= "<p>" . self::childrenToHtml($element) . "</p>";
                break;
            case "cs":
                $html .= "<table>" . self::childrenToHtml($element) . "</table>";
                break;
            case "h":
                if ($element->Parent->Name == "cs") {
                    $html .= "<caption>" . self::childrenToHtml($element) . "</caption>";
                } else {
                    $html .= "<h2>" . self::childrenToHtml($element) . "</h2>";
                }
                break;
            case "pt":
                $html .= '<tr><th>'. trim($element->Param[0],'"') . '</th><td>' . self::childrenToHtml($element) . '</td></tr>';
                break;
            case "l":
                $html .= "<a href={$element->Param[0]}>" . self::childrenToHtml($element) . '</a>';
                break;
            case "sl":
                $html .= '<sup>' . trim($element->Param[0],'"') . '</sup>';
                break;
            case "b":
                $html .= "<b>" . self::childrenToHtml($element) . "</b>";
                break;
            case "i":
                $html .= "<i>" . self::childrenToHtml($element) . "</i>";
                break;
            case "n":
                $html .= "<br/>";
                break;
            case "s":
                $html .= "<div>" . self::childrenToHtml($element) . "</div>";
                break;
            case "p":
                $html .= "<p>" . self::childrenToHtml($element) . "</p>";
                break;
            case "nl":
                $html .= "<ol>" . self::childrenToHtml($element) . "</ol>";
                break;
            case "bl":
                $html .= "<ul>" . self::childrenToHtml($element) . "</ul>";
                break;
            case "li":
            case "si":
                $html .= "<li>" . self::childrenToHtml($element) . "</li>";
                break;
            case "text":
                $html .= $element->Content;
                break;
        }
        return $html;
    }
}
class CoreElement
{
    public static $singleTags = ["WiML", "n", "img", "sl"];
    public static $notNullParam = ["pt"];
    public static $requireData = ["title", "card", "body", "source", "cs", "s", "bl", "nl", "l", "h", "d", "pt", "p", "li", "si", "b", "i"];
    public static $requiredTagsCount = [
        "core" => ["WiML" => "1", "title" => "1", "card" => "?", "body" => "1", "source" => "?"],
        "card" => ["img" => "?", "d" => "?"],
        "source" => ["si" => "+"],
        "cs" => ["h" => "1"],
        "s" => ["h" => "1"],
        "bl" => ["li" => "+"],
        "nl" => ["li" => "+"]
    ];
    public static $ElementsAllowedTags = [
        "core" => ["WiML", "title", "card", "body", "source"],
        "WiML" => [],
        "card" => ["img", "d", "cs"],
        "body" => ["s"],
        "source" => ["si"],
        "cs" => ["h", "pt"],
        "s" => ["s", "img", "h", "p", "bl", "nl"],
        "bl" => ["li"],
        "nl" => ["li"],
        "title" => ["b", "i", "text"],
        "l" => ["text"],
        "sl" => [],
        "h" => ["b", "i", "text"],
        "d" => ["b", "i", "l", "sl", "text"],
        "pt" => ["img", "l", "sl", "b", "i", "n", "text"],
        "p" => ["l", "sl", "b", "i", "n", "text"],
        "li" => ["b", "i", "n", "l", "sl", "text"],
        "si" => ["b", "i", "l", "text"],
        "b" => ["i", "n", "text"],
        "i" => ["b", "n", "text"],
        "n" => [],
        "img" => []
    ];
    public $title = "";
    public $Card;
    public $Body;
    public $Source;
    public $Data;
    public $isProcessed;
    public static $tagWithOutParam = "/\[\w+\/?\]/";
    public static $tagWithShortParam = "/\[\w+=.+\/?\]/";
    public static $tagWithParam = "/\[\w+(?:\s+\w+=\"[^\"]+\")+\/?\]/";
    public static $tagName = "/\w+/";
    public function __construct($Data)
    {
        $this->Data = $Data;
    }
    public static function TagToNode($tag, $data, $parent, $coreElement)
    {
        $name = preg_match_of_normal_human(self::$tagName, $tag)[0];
        if (in_array($name, self::$requireData) && empty($data)) {
            throw new Exception("$name mast contains data");
        }
        if (preg_match(self::$tagWithOutParam, $tag)) {
            $elem = new Element($parent, $coreElement, $name, null, $data);
        } elseif (preg_match(self::$tagWithShortParam, $tag)) {
            $t = explode("=", substr($tag, 1, -1));
            $t[1] = rtrim($t[1], "]");
            $elem = new Element($parent, $coreElement, $name, [$t[1]], $data);
        } elseif (preg_match(self::$tagWithParam, $tag)) {
            $t = split(trim(str_replace("]","", substr($tag, strlen($name) + 1, -1)), '/')," ");
            $elem = new Element($parent, $coreElement, $name, $t, $data);
        } else {
            throw new Exception($tag);
        }
        if (in_array($name, self::$notNullParam)) {
            if (!is_null($elem->Param)) {
                foreach ($elem->Param as $param) {
                    if (empty($param)) {
                        throw new Exception("require not empty parameters $tag");
                    }
                }
            } else {
                throw new Exception("require not empty parameters, $elem->Param $elem->Name, $elem->Content, $tag");
            }
        }
        return $elem;
    }
    public static function ProcessNode($data, $name, &$elements, $parent, $core)
    {   
        $elem = [];
        $data = preg_replace("/\s+/", " ", $data);
        if ($data == "") {
            return true;
        }
        $i = 0;
        $tagStartIndex = 0;
        $isTagName = false;
        $branchStartIndex = 0;
        $isBranch = false;
        $branchRootTag = "";
        $isText = false;
        $textIndex = 0;
        $line = 1;
        $chr = 1;
        while (true) {
            if ($i > strlen($data) - 1) {
                if (array_key_exists($name, self::$requiredTagsCount)) {
                    foreach (self::$requiredTagsCount[$name] as $rule => $value) {
                        $count = count(array_filter($elements, function ($x) use ($rule) {
                            return $x->Name == $rule;
                        }));
                        switch ($value) {
                            case "1":
                                if ($count != 1) {
                                    throw new Exception("only one $rule tag allowed in $name");
                                }
                                break;
                            case "?":
                                if ($count > 1) {
                                    throw new Exception("no more than one $rule tag allowed in $name");
                                }
                                break;
                            case "+":
                                if ($count < 1) {
                                    throw new Exception("requires at least one tag $rule in $name");
                                }
                                break;
                            default:
                                throw new Exception("there is no such rule as $value");
                        }
                    }
                }
                break;
            }
            if ($data[$i] == "[") {
                if ($isText) {
                    $tmp = new Element($parent, $core, "text", null, substr($data, $textIndex, $i - $textIndex));
                    $tmp->isProcessed = true;
                    $elements[] = $tmp;
                    $isText = false;
                }
                $isTagName = true;
                $tagStartIndex = $i;
            }
            if ($i == strlen($data) - 1) {
                if ($isText) {
                    $tmp = new Element($parent, $core, "text", null, substr($data, $textIndex, $i - $textIndex + 1));
                    $tmp->isProcessed = true;
                    $elements[] = $tmp;
                }
            } else {
                if (!$isText && !$isTagName && !$isBranch && !preg_match("/\s/", $data[$i])) {
                    $isText = true;
                    $textIndex = $i;
                }
            }
            if ($data[$i] == "\n") {
                $line++;
                $chr = 1;
            }
            if ($data[$i] == "]") {
                if (!$isTagName) {
                    throw new Exception();
                }
                $temp = substr($data, $tagStartIndex + 1, $i - $tagStartIndex - 1);
                if ($temp[0] == "/") {
                    $t = array_pop($elem);
                    if (preg_match(self::$tagName, $t, $matches) && preg_match(self::$tagName, $temp, $matches2) && $matches[0] != $matches2[0]) {
                        throw new Exception("$temp $t $line $chr");
                    }
                    if (count($elem) == 0) {
                        $rowBranchData = substr($data, $branchStartIndex, $i - $branchStartIndex + 1);
                        $j = 0;
                        while ($rowBranchData[$j] != "]" || $j > strlen($rowBranchData) - 1) {
                            $j++;
                        }
                        $tag = substr($rowBranchData, 0, $j + 1);
                        $param = [];
                        $rowBranchData = substr($rowBranchData, $j + 1, strlen($rowBranchData) - (strlen($branchRootTag) + strlen(preg_match_of_normal_human(self::$tagName, $branchRootTag)[0]) + 5));
                        $isBranch = false;
                        $elements[] = self::TagToNode($tag, $rowBranchData, $parent, $core);
                    }
                } elseif ($temp[strlen($temp) - 1] != "/") {
                    array_push($elem, $temp);
                    if ($isBranch == false) {
                        preg_match(self::$tagName, $temp, $matches);
                        if (!in_array($matches[0], self::$ElementsAllowedTags[$name])) {
                            throw new Exception("$temp $name $line $chr");
                        }
                        $branchStartIndex = $tagStartIndex;
                        $isBranch = true;
                        $branchRootTag = $temp;
                    }
                } else {
                    preg_match(self::$tagName, $temp, $matches);
                    if (!in_array($matches[0], self::$singleTags)) {
                        throw new Exception($temp);
                    }
                    if (!$isBranch) {
                        preg_match(self::$tagName, $temp, $matches);
                        if (!in_array($matches[0], self::$ElementsAllowedTags[$name])) {
                            throw new Exception(preg_match_of_normal_human(self::$tagName, $temp)[0] . " $name $line $chr");
                        }
                        $elements[] = self::TagToNode("[$temp]", "", $parent, $core);
                    }
                }
                $isTagName = false;
            }
            $i++;
            $chr++;
        }
        return true;
    }
    public static function ProcessElement($element)
    {
        return self::ProcessNode($element->Content, $element->Name, $element->Children, $element, $element->CoreElement);
    }
    public static function ProcessTree($node)
    {
        self::ProcessElement($node);
        foreach ($node->Children as $elem) {
            if (!$elem->isProcessed) {
                self::ProcessTree($elem);
            }
        }
    }
    public static function PrintTree($node)
    {
        echo $node . "\n";
        foreach ($node->Children as $elem) {
            self::PrintTree($elem);
        }
    }
    public function Process()
    {
        self::ProcessNode($this->Data, "core", $el, null, $this);
        $this->title = $el[1]->Content;
        $this->Card = $el[2];
        $this->Body = $el[3];
        $this->Source = $el[4];
        if (!is_null($this->Source)) {
            self::ProcessTree($this->Source);
        }
        if (!is_null($this->Card)) {
            self::ProcessTree($this->Card);
        }
        self::ProcessTree($this->Body);
        return true;
    }
    public function PrintAST()
    {
        echo $this->title . "\n";
        if (!is_null($this->Card)) {
            self::PrintTree($this->Card);
        }
        self::PrintTree($this->Body);
        if (!is_null($this->Source)) {
            self::PrintTree($this->Source);
        }
    }
}
