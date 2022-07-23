Imports System.Text
Imports System.Net
Imports System.Security.Cryptography



Public Class Spot

    Public Shared Property Carpeta As String = ""

    Public Shared Property UltimoError As String = ""

    Public Shared URL As String = "https://api.binance.com"

    Public Precios As ClasesAuxiliares.API_Binance_Precios

    Public Ordenes As ClasesAuxiliares.API_Binance_Ordenes

    Public Consultas As ClasesAuxiliares.API_Binance_Consultas

    Public Sub New(Carpeta As String)

        Spot.Carpeta = Carpeta

        Precios = New ClasesAuxiliares.API_Binance_Precios

        Ordenes = New ClasesAuxiliares.API_Binance_Ordenes

        Consultas = New ClasesAuxiliares.API_Binance_Consultas


    End Sub


    Public Shared Function ConvertirAFechaBinance(fecha As DateTime) As Long
        Return CLng(fecha.ToUniversalTime.Subtract(New DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds)
    End Function


End Class

Namespace ClasesAuxiliares

    Public Class API_Binance_Precios

        Public Function ObtenerPuntas(Simbolo As String, limite As Integer) As String
            Dim Solicitud As New Solicitud() With {
                .EndPont = "/api/v3/depth",
                .Metodo = "GET",
                .Firmar = False,
                .ForzarEnviarAPIKEy = False,
                .TimeStamp = False
                }
            Solicitud.Parametros.Add("symbol", Simbolo)
            Solicitud.Parametros.Add("limit", limite)

            Dim respuesta = Solicitud.Send
            Return respuesta
        End Function





    End Class


    Public Class API_Binance_Ordenes

    End Class

    Public Class API_Binance_Consultas

        Public Function ObtenerReglas() As String
            Dim solicitud As New Solicitud() With {
                .EndPont = "/api/v3/exchangeInfo",
                .Metodo = "GET"
            }

            Dim Respuesta As String = solicitud.Send

            Try
                IO.File.WriteAllText(Spot.Carpeta & "\Reglas.json", Respuesta)
            Catch ex As Exception

            End Try

            Return Respuesta

        End Function

    End Class

    Public Class Solicitud



        Public Property EndPont As String = ""
        Public Property Metodo As String = ""
        Public Property Firmar As Boolean = False

        Public Property ForzarEnviarAPIKEy As Boolean = False

        Public Property TimeStamp As Boolean = False

        Public Property RecvWindows As Integer = 5000

        Public Property Parametros As Dictionary(Of String, String)

        Public Sub New()
            Parametros = New Dictionary(Of String, String)

        End Sub

        Public Function Send() As String
            Spot.UltimoError = ""
            Try
                Dim Respuesta As String = ""

                Dim sParametros As String = ""
                If TimeStamp Then
                    sParametros = "recvWindow=" & RecvWindows & "&timestamp=" & Spot.ConvertirAFechaBinance(Now)
                End If
                For Each par As KeyValuePair(Of String, String) In Parametros
                    sParametros &= IIf(sParametros.Length = 0, "", "&") & par.Key & "=" & par.Value
                Next

                If Firmar Then
                    Dim Firma As String = Cifrar(sParametros, Binance.Configuracion.API_Secret)
                    sParametros &= "&signature=" & Firma
                End If

                Dim sURL As String = Spot.URL + EndPont
                Dim bParametros() As Byte = Nothing

                If Metodo = "GET" Then
                    sURL &= "?" & sParametros
                Else
                    bParametros = Encoding.UTF8.GetBytes(sParametros)
                End If

                Dim solicitud As WebRequest = WebRequest.Create(sURL)
                solicitud.Method = Metodo

                If Metodo <> "GET" Then
                    If bParametros.Length > 0 Then
                        solicitud.ContentType = "application/x-www-form-urlencoded"
                        solicitud.ContentLength = bParametros.Length
                        Using flujo = solicitud.GetRequestStream
                            flujo.Write(bParametros, 0, bParametros.Length)
                            flujo.Close()
                        End Using
                    End If
                End If

                If Firmar Or ForzarEnviarAPIKEy Then
                    solicitud.Headers.Add("X-MBX-APIKEY", Binance.Configuracion.API_Key)
                End If

                Using flujo = solicitud.GetResponse.GetResponseStream
                    Using lector As New IO.StreamReader(flujo)
                        Respuesta = lector.ReadToEnd
                    End Using
                End Using


                Return Respuesta
            Catch ex As Exception
                Spot.UltimoError = ex.Message
                Return ""
            End Try
        End Function

        Private Function Cifrar(dato As String, Clave_Privada As String) As String
            Dim dato_bytes() As Byte = Encoding.UTF8.GetBytes(dato)
            Dim clave_bytes() As Byte = Encoding.UTF8.GetBytes(Clave_Privada)

            Dim hmac As HMACSHA256 = New HMACSHA256(clave_bytes)
            Dim codificado() As Byte = hmac.ComputeHash(dato_bytes)
            Dim respuesta As String = BitConverter.ToString(codificado)
            respuesta = respuesta.Replace("-", "").ToLower
            Return respuesta
        End Function
    End Class


End Namespace