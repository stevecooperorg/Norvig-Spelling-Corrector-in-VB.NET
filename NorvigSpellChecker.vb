Option Strict Off
'import re, collections
Imports System.Text.RegularExpressions
Imports System.Collections.Generic

Module NorvigSpellChecker

    Dim NWORDS

    ' alphabet = 'abcdefghijklmnopqrstuvwxyz'
    Dim alphabet As String = "abcdefghijklmnopqrstuvwxyz"

    ' def words(text): 
    Function words(ByVal text)
        ' return re.findall('[a-z]+', text.lower()) 
        Return From match In Regex.Matches(text.ToLower(), "[a-z]+").Cast(Of Match)() Select match.Value
    End Function


    ' def train(features):
    Function train(ByVal features)
        ' model = collections.defaultdict(lambda: 1)
        Dim model = New DefaultDict(Function() 1)
        ' for f in features:
        For Each f In features
            ' model[f] += 1
            model(f) += 1
        Next
        ' return model
        Return model
    End Function

    ' def edits1(word):
    Function edits1(ByVal word) As IEnumerable(Of Object)
        ' splits   = [(word[:i], word[i:]) for i in range(len(word) + 1)]
        Dim splits = From i In Enumerable.Range(0, len(word)) Let a = word.Substring(0, i), b = word.Substring(i) Select a, b
        ' deletes   = [a + b[1:] for a, b in splits if b]
        Dim deletes = From split In splits Where ifs(split.b) Select split.a + split.b.Substring(1)
        ' transposes = [a + b[1] + b[0] + b[2:] for a, b in splits if len(b)>1]
        Dim transposes = From split In splits Where len(split.b) > 1 Select split.a + split.b.Substring(1, 1) + split.b.Substring(0, 1) + split.b.Substring(2)
        ' replaces   = [a + c + b[1:] for a, b in splits for c in alphabet if b]
        Dim replaces = From split In splits From c In alphabet Where ifs(split.b) Select split.a + c + split.b.Substring(1)
        ' inserts    = [a + c + b     for a, b in splits for c in alphabet]
        Dim inserts = From split In splits From c In alphabet Select split.a + c + split.b
        ' return set(deletes + transposes + replaces + inserts)
        Return deletes.Union(transposes).Union(replaces).Union(inserts).Distinct()
    End Function

    ' def known_edits2(word):
    Function known_edits2(ByVal word)
        'set(e2 for e1 in edits1(word) for e2 in edits1(e1) if e2 in NWORDS)
        Return edits1(word).SelectMany(Function(e1) edits1(e1)).Distinct().Where(Function(e) NWORDS.ContainsKey(e))
    End Function

    ' def known(words): 
    Function known(ByVal words As IEnumerable)
        ' return set(w for w in words if w in NWORDS)
        Return From w In words Where NWORDS.ContainsKey(w) Select w
    End Function

    ' def correct(word):
    Function correct(ByVal word)
        ' candidates = known([word]) or known(edits1(word)) or known_edits2(word) or [word]
        Dim candidates = ORR(Function() known(New String() {word}), Function() known(edits1(word)), Function() known_edits2(word), Function() New String() {word})
        ' return max(candidates, key=NWORDS.get)
        Return candidates.OrderBy(Function(candidate) NWORDS(candidate)).FirstOrDefault()
        'Return candidates.Math.Max(candidates, key = NWORDS.get)
    End Function


    Sub Main()
        ' NWORDS = train(words(file('big.txt').read()))
        NWORDS = train(words(System.IO.File.ReadAllText("c:\big.txt")))
        
        ' CODE TO TEST; RETURNS 'visual basic may not entirely suck'
        Dim corrected = String.Join(" ", (From word In "visaul bsaic may not etnirely sukc".Split() Select (correct(word).ToString())).ToArray())
        Console.WriteLine(corrected)
    End Sub

    '
    ' PYTHONIC FUNCTIONS
    '

    Public Function ifs(ByVal s As String) As Boolean
        Return Not s Is Nothing
    End Function

    Function len(ByVal s As String) As Integer
        If s Is Nothing Then
            Return 0
        Else
            Return s.Length
        End If
    End Function

    '
    ' OR shortcut
    '
    Public Function ORR(ByVal ParamArray funcs() As Func(Of Object)) As IEnumerable(Of Object) ' ByVal list As Object, ByVal continues As Func(Of Object))
        For Each func In funcs
            Dim list = func()
            If (Enumerable.Count(Of Object)(list) > 0) Then
                Return list
            End If
        Next
        Return New ArrayList()
    End Function


End Module

'
' VB equivalent of pythons's default dictionary.
'
Public Class DefaultDict

    Delegate Function ProvideDefaultValue() As Integer

    Dim _missingLambda As ProvideDefaultValue
    Dim _items As Dictionary(Of String, Integer)

    Public Sub New(ByVal lambda As ProvideDefaultValue)
        _missingLambda = lambda
        _items = New Dictionary(Of String, Integer)
    End Sub

    Public Function ContainsKey(ByVal s As String) As Boolean
        Return _items.ContainsKey(s)
    End Function

    Default Public Property Items(ByVal o As String) As Integer
        Get
            If Not _items.ContainsKey(o) Then
                _items.Add(o, _missingLambda())
            End If
            Return _items.Item(o)
        End Get
        Set(ByVal value As Integer)
            If Not _items.ContainsKey(o) Then
                _items.Add(o, _missingLambda())
            Else
                Dim item = _items.Item(o)
                _items.Remove(o)
                _items.Add(o, item + 1)
            End If
        End Set
    End Property
End Class
